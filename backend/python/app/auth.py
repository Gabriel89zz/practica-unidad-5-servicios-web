from fastapi import Header, HTTPException, status
from typing import Optional

VALID_BEARER_TOKEN = "Bearer test_token_2026"
VALID_API_KEY = "secret_key_cs"
SOAP_VALID_USER = "admin"
SOAP_VALID_PASS = "admin_pass_2026"

def verify_rest_auth(
    authorization: Optional[str] = Header(None, alias="Authorization"),
    x_api_key: Optional[str] = Header(None, alias="X-API-Key")
):
    authorized = False
    if authorization and authorization.strip() == VALID_BEARER_TOKEN:
        authorized = True
    elif x_api_key and x_api_key.strip() == VALID_API_KEY:
        authorized = True

    if not authorized:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail={
                "error": "Unauthorized",
                "message": "Token Bearer o API-Key inválida o ausente. Use 'Authorization: Bearer test_token_2026' o 'X-API-Key: secret_key_cs'",
                "statusCode": 401
            },
            headers={"WWW-Authenticate": "Bearer"}
        )
    return True
