var config = {
		"environment": "mypurecloud.com",		
		"redirectUri" : [window.location.protocol, '//', window.location.host, window.location.pathname].join(''),
		"wrapupServiceUri": [window.location.protocol, '//', window.location.host, '/api/wrapupcode/update'].join(''),
		"maxAttempts": 3,
		"clientIdAttributeName": "Client_Id",
		"clientNameAttributeName": "Client_Name"
	};