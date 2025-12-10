from flask import Blueprint, request, jsonify, session, redirect
from flask_login import login_user, current_user, logout_user
import logging

logger = logging.getLogger(__name__)

abp_auth_bp = Blueprint('abp_auth', __name__, url_prefix='/api/v1/auth')


@abp_auth_bp.route('/abp-sso', methods=['POST'])
def abp_sso():
    """SSO endpoint - API için"""
    try:
        from superset import security_manager, db
        
        data = request.get_json()
        logger.info(f"=== ABP SSO START === Data: {data}")
        
        username = data.get('username')
        if not username:
            return jsonify({'success': False, 'error': 'No username'}), 400
        
        email = data.get('email', f"{username}@supersetabp.com")
        first_name = data.get('first_name', username)
        last_name = data.get('last_name', '')
        roles = data.get('roles', ['user'])
        
        if isinstance(roles, str):
            roles = [roles]
        
        # Role mapping
        role_mapping = {'admin': 'Admin', 'user': 'Alpha', 'viewer': 'Gamma'}
        superset_role_name = 'Alpha'
        
        for role in roles:
            if role.lower() in role_mapping:
                superset_role_name = role_mapping[role.lower()]
                break
        
        logger.info(f"User: {username}, Role: {superset_role_name}")
        
        # Find or create user
        user = security_manager.find_user(username=username)
        
        if not user:
            logger.info(f"Creating user: {username}")
            role = security_manager.find_role(superset_role_name)
            if not role:
                return jsonify({'success': False, 'error': f'Role not found: {superset_role_name}'}), 500
            
            user = security_manager.add_user(
                username=username,
                first_name=first_name,
                last_name=last_name,
                email=email,
                role=[role],
                password='sso_no_password'
            )
            logger.info(f"✅ User created: {username}")
        else:
            logger.info(f"User exists: {username}")
            user.first_name = first_name
            user.last_name = last_name
            user.email = email
            
            role = security_manager.find_role(superset_role_name)
            if role:
                user.roles = [role]
                db.session.commit()
            
            logger.info(f"✅ User updated: {username}")
        
        # Login
        login_success = login_user(user, remember=True, force=True)
        if not login_success:
            logger.error(f"❌ Login failed: {username}")
            return jsonify({'success': False, 'error': 'Login failed'}), 500
        
        session.permanent = True
        session.modified = True
        
        logger.info(f"✅ LOGIN SUCCESS: {username}")
        
        return jsonify({'success': True, 'username': username, 'role': superset_role_name}), 200
        
    except Exception as e:
        logger.error(f"❌ SSO ERROR: {e}", exc_info=True)
        return jsonify({'success': False, 'error': str(e)}), 500


@abp_auth_bp.route('/abp-login', methods=['GET'])
def abp_login_page():
    """
    Login page endpoint - Yeni sekme için
    Query string'den user bilgilerini alır, login yapar, ana sayfaya redirect eder
    """
    try:
        from superset import security_manager, db
        
        username = request.args.get('username')
        email = request.args.get('email', f"{username}@supersetabp.com")
        first_name = request.args.get('first_name', username)
        roles_str = request.args.get('roles', 'user')
        
        logger.info(f"=== LOGIN PAGE REQUEST === Username: {username}")
        
        if not username:
            return "<h1>Error: No username provided</h1>", 400
        
        roles = roles_str.split(',') if roles_str else ['user']
        
        # Role mapping
        role_mapping = {'admin': 'Admin', 'user': 'Alpha', 'viewer': 'Gamma'}
        superset_role_name = 'Alpha'
        
        for role in roles:
            if role.lower().strip() in role_mapping:
                superset_role_name = role_mapping[role.lower().strip()]
                break
        
        logger.info(f"Mapped role: {superset_role_name}")
        
        # Find or create user
        user = security_manager.find_user(username=username)
        
        if not user:
            logger.info(f"Creating user: {username}")
            role = security_manager.find_role(superset_role_name)
            if not role:
                return f"<h1>Error: Role {superset_role_name} not found</h1>", 500
            
            user = security_manager.add_user(
                username=username,
                first_name=first_name,
                last_name='',
                email=email,
                role=[role],
                password='sso_no_password'
            )
            logger.info(f"✅ User created: {username}")
        else:
            logger.info(f"User exists: {username}")
        
        # Login
        login_success = login_user(user, remember=True, force=True)
        if not login_success:
            logger.error(f"❌ Login failed: {username}")
            return "<h1>Error: Login failed</h1>", 500
        
        session.permanent = True
        session.modified = True
        
        logger.info(f"✅ LOGIN SUCCESS: {username}")
        
        # Redirect to home
        return redirect('/')
        
    except Exception as e:
        logger.error(f"❌ LOGIN PAGE ERROR: {e}", exc_info=True)
        return f"<h1>Error: {str(e)}</h1>", 500


@abp_auth_bp.route('/abp-check', methods=['GET'])
def abp_check():
    """Check authentication status"""
    is_auth = current_user.is_authenticated
    username = current_user.username if is_auth else None
    
    logger.info(f"Auth check: {is_auth}, user: {username}")
    
    return jsonify({'authenticated': is_auth, 'username': username})


@abp_auth_bp.route('/abp-logout', methods=['POST'])
def abp_logout():
    """Logout endpoint"""
    username = current_user.username if current_user.is_authenticated else 'none'
    logout_user()
    session.clear()
    
    logger.info(f"✅ Logout: {username}")
    
    return jsonify({'success': True})