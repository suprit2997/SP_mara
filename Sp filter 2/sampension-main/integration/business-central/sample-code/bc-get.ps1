# Define rest paths
$BASEURL = 'https://api.businesscentral.dynamics.com/v2.0/'
$TENANT = '7cb704f0-946d-416b-9582-4926034e2bb2'
$ENVIRONMENT = 'Sandbox_SP_Test'
$COMPANY = 'Sampension Master'
$PUBLISHER = 'sampension'
$GROUP = 'api'
$VERSION = 'v1.0'


# Define auth body paths
$grant_type = 'client_credentials'
$client_id = '9622551a-2839-4f5a-9c80-3ab87f610cf5'
$client_secret = '1mP8Q~5EV~uKeD5dHJXHaIGR_2mjThFxO6mOmcwV'
$scope = 'https://api.businesscentral.dynamics.com/.default'


# Define entity
$ENTITYSETNAME = 'summarizedGLEntries'

# Auth API:
# POST https://login.microsoftonline.com/$TENANT/oauth2/v2.0/token
$auth_url = 'https://login.microsoftonline.com/'+$TENANT+'/oauth2/v2.0/token'
$auth_body = 'grant_type='+$grant_type+'&'+'client_id='+$client_id+'&'+'client_secret='+$client_secret+'&'+'scope='+$scope
$auth_token = Invoke-RestMethod -Uri $auth_url -Method POST -Body $auth_body -ContentType 'application/x-www-form-urlencoded'
$access_token = $auth_token.access_token

# Custom API:
# GET https://api.businesscentral.dynamics.com/v2.0/$COMPANY/api/$PUBLISHER/$GROUP/$VERSION/$ENTITYSETNAME
$custom_url = $BASEURL+$ENVIRONMENT+'/api/'+$PUBLISHER+'/'+$GROUP+'/'+$VERSION+'/'+$ENTITYSETNAME+'?$expand=dimensionSetEntries&$filter=postingdate gt ''2024-01-01'''
$headers = @{"Company" = "$COMPANY"; "Authorization" = "Bearer $access_token"; "Content-Type" = "application/Json"}
$Response = Invoke-RestMethod -Uri $custom_url -Method GET -Headers $headers
$Response.value

# Convert response to JSON and write output
$Response | ConvertTo-Json
$custom_url
$headers