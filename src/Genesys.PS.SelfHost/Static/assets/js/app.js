var callChangeTime
var needsToEndCall
var currentQueueId
var gcToken
var conversationId
var agentParticipantId
var initialWrapupCode
var wrapupCodeEntity = {}
var userQueues = []

$(document).ready(function () {
	hideMessages()
	$("#updateWrapupCodeDiv").hide();
	$("#buttonsDiv").hide()	
			
	if(window.location.hash) 
	{	
		gcToken = getParameterByName('access_token', window.location.hash);
		let userData = getUserData(gcToken);
		let activeConversation = getActiveInteraction(gcToken)					
		location.hash='';			
		Promise.all([userData, activeConversation]).then(result => {
			console.log(result, "userData & activeConversation"); 
			const user = result[0]
			const conversations = result[1]
			if(conversations.length > 0){
				const activeConversation = conversations[0]
				conversationId = activeConversation.id									
				const agentParticipant = activeConversation.participants.find(p => p.userId == user.id && p.connectedTime && !p.endTime)
								
				if(!agentParticipant){
					console.info("The agent is not connected!")
					showMessage("warning", "The agent have not answered the call")
					return
				}

				agentParticipantId = agentParticipant.id
				const customerParticipant = activeConversation.participants.find(p => p.purpose == "customer" || p.purpose == "external");
				const acdPartipant = activeConversation.participants.find(p => p.purpose == "acd")

				let queueStartTime
				if(acdPartipant){
					queueStartTime = acdPartipant.connectedTime
					if(!queueStartTime){
						queueStartTime = acdPartipant.connectedTime
					}					
				}
				else{
					queueStartTime = agentParticipant.startTime
				}

				console.log(agentParticipant, "agentParticipant")
				console.log(customerParticipant, "customerParticipant")				
				console.log(user.locationName, "locationName")

				currentQueueId = agentParticipant.queueId
				console.info(currentQueueId, "currentQueueId")					
				getQueuesByUser(gcToken, user.id).then((queuesByUserResult) => 
				{
					userQueues = queuesByUserResult						
					const queue = userQueues.find(i => i.id == currentQueueId)
					
					if(!customerParticipant.attributes)
					{
						customerParticipant.attributes = {}
					}
					if(queue){							
						updateCurrentQueueText(queue.name)
						wrapupCodeEntity = { CallIDKey: conversationId, AgentID: user.email, ANI: customerParticipant.ani, DNIS: customerParticipant.dnis, ClientName: customerParticipant.attributes[config.clientNameAttributeName]??null, ClientId: customerParticipant.attributes[config.clientIdAttributeName]??null, Skill: queue.name, IntConnStartTime: agentParticipant.connectedTime, queueStartTime: queueStartTime??null, QueueEndTime: agentParticipant.connectedTime, TimeStamp1: agentParticipant.connectedTime, Location: user.locationName??null } 
						
						saveWrapupCodeAsync(wrapupCodeEntity).then((result) => {
							populateQueues(currentQueueId)
							$("#buttonsDiv").show()
						}).catch(function(e) {
							showMessage("error", e)
						})				
					}
					else{
						console.error(`The queue was not found ${currentQueueId} in userQueues: ${JSON.stringify(userQueues)}`)
					}
				}
				)
				.catch((error) => {
					console.error(`Error getting queues by user: ${error}`)
					showMessage("error", error)
				})
				
			}
			else{
				console.info("There is no an active conversation!")
				showMessage("warning", "The user has no an active conversation!")
			}
			}).catch(function(e) {
			showMessage("error", e)
		})		
	}
	else
	{
		const clientId = getParameterByName('clientId', window.location.search);	
			
		var queryStringData = {
			response_type : "token",
			client_id : clientId,
			redirect_uri : config.redirectUri
		}		
		window.location.replace("https://login." + config.environment + "/authorize?" + jQuery.param(queryStringData));
	}

});		
  
function changeCall() {		
	$("#buttonsDiv").hide()
	$("#updateWrapupCodeDiv").show();
	$("#queuesDiv").show()	
	callChangeTime = new Date();
	console.info(`Change call: ${callChangeTime}`)
}

function endCall() {
	$("#buttonsDiv").hide()
	$("#updateWrapupCodeDiv").show()
	$("#queuesDiv").hide()	
	needsToEndCall = true
	callChangeTime = new Date();
	console.info(`End call needsToEndCall: ${needsToEndCall}, callChangeTime: ${callChangeTime}`)
}

function cancel() {
	needsToEndCall = false
	callChangeTime = null
	$("#updateWrapupCodeDiv").hide()
	$("#buttonsDiv").show()
}

function updateWrapupCode() {		
	if(callChangeTime){
		const currentTime = new Date().toISOString()
		wrapupCodeEntity.IntConnEndTime = callChangeTime.toISOString()
		wrapupCodeEntity.WrapupStartTime = callChangeTime.toISOString()
		wrapupCodeEntity.WrapupEndTime = currentTime
		wrapupCodeEntity.CallCode = $("#wrapupCodesCtrl option:selected").text()
		const selectedQueue = $("#queuesCtrl").val()

		if(!initialWrapupCode){
			initialWrapupCode =  $("#wrapupCodesCtrl").val()
			console.info(`initialWrapupCode -> ${initialWrapupCode} - ${wrapupCodeEntity.CallCode} - queue: ${wrapupCodeEntity.Skill}`)
		}

		if(!selectedQueue && !needsToEndCall){
			showMessage("warning", "Select a queue", 2000)			
			return	
		}

		if (!wrapupCodeEntity.CallCode) {	
			showMessage("warning", "Select a wrap-up code", 2000)			
			return
		}	

		saveWrapupCodeAsync(wrapupCodeEntity).then((result) => {
			console.info(`needsToEndCall: ${needsToEndCall}`)
			if(needsToEndCall){
				setWrapupCode(gcToken, conversationId, agentParticipantId, initialWrapupCode).then((result) => 
				{
					$("#updateWrapupCodeDiv").hide()
					$("#currentQueue").hide()	
				}
				)
				.catch((error) => {
					console.error(`Error setWrapupCode: ${error}`)
				})
									
			}
		}).catch(function(e) {
			showMessage("error", e)
		})
		
					
		if(selectedQueue && selectedQueue != currentQueueId){			
			currentQueueId = selectedQueue
			const queue = userQueues.find(i => i.id == currentQueueId)
			updateCurrentQueueText(queue.name)
			console.log(queue, "updateWrapupCode-queue")
			wrapupCodeEntity = { CallIDKey: wrapupCodeEntity.CallIDKey, AgentID: wrapupCodeEntity.AgentID, ANI: wrapupCodeEntity.ANI, DNIS: wrapupCodeEntity.DNIS, ClientName: wrapupCodeEntity.ClientName, ClientId: wrapupCodeEntity.ClientId,  Skill: queue.name, IntConnStartTime: currentTime, queueStartTime: currentTime, QueueEndTime: currentTime, TimeStamp1: currentTime, Location: wrapupCodeEntity.Location } 
			saveWrapupCodeAsync(wrapupCodeEntity).then((result) => {
				populateQueues(currentQueueId)
				$("#updateWrapupCodeDiv").hide()
				$("#buttonsDiv").show()	
			}
			)
			.catch((error) => {
				console.error(`Error saveWrapupCode: ${error}`)
				$("#updateWrapupCodeDiv").hide()
				$("#buttonsDiv").show()	
			})						
			
		}
	}	
}

async function populateQueues(hiddenQueueId){	
	const $queuesCtrl = $("#queuesCtrl");
	$queuesCtrl.empty()
	$queuesCtrl.append($("<option />").val('').text('Change Call To'));
	console.log(userQueues, "populateQueues-userQueues")
	console.log(hiddenQueueId, "populateQueues-hiddenQueueId")

	for (let queue of userQueues){							
		console.log(queue, "populateQueues-queue")
		if(hiddenQueueId != queue.id){
			$queuesCtrl.append($("<option />").val(queue.id).text(queue.name));
		}	
	}

	getWrapupCodesByQueue(gcToken, hiddenQueueId).catch(function(e) {
		sleep(3000).then(
			getWrapupCodesByQueue(gcToken, hiddenQueueId).catch(function(e) {
				showMessage("error", e)
			})
		)
	})
	
}

function getParameterByName(name, data) {
	name = name.replace(/[\[]/, "\\[").replace(/[\]]/, "\\]");
	let regex = new RegExp("[\\#&?]" + name + "=([^&#?]*)"),
	results = regex.exec(data);
	return results === null ? "" : decodeURIComponent(results[1].replace(/\+/g, " "));
}

function getUserData(token)
{
	return new Promise((resolve, reject) => {
		let url = "https://api." + config.environment + "/api/v2/users/me?expand=locations,presence,routingStatus&fl=*" 
		$.ajax({
			url: url,
			type: "GET",
			beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
			success: function (result) {
				console.log(result, "getUserData")
				const userLocations = result.locations

				if(userLocations && userLocations.length > 0 && userLocations[0].locationDefinition?.id){
					const locationId = userLocations[0].locationDefinition.id
					
					url = "https://api." + config.environment + "/api/v2/locations/" + locationId
					$.ajax({
						url: url,
						type: "GET",
						timeout: 60000,
						beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
						success: function (userLocationResult) {
							console.log(userLocationResult, "getUserLocation-success")
							result.locationName = userLocationResult.name								
							resolve(result)							
						},
						error: function (request) {
							console.log("getUserLocation-error", "url: " + url + ", detail: " + JSON.stringify(request))
							reject("getUserLocation-error -> " + JSON.stringify(request))
						}
					});
					
				}	
				else{
					resolve(result)
				}
				
			},
			error: function (request) {
				console.log("getUserData", "error -> url: " + url + ", detail: " + JSON.stringify(request));
				reject("getUserData-error -> " + JSON.stringify(request))

			}
		});
	});
}

function getQueuesByUser(token, userId)
{
	return new Promise((resolve, reject) => {		
		const url = `https://api.${config.environment}/api/v2/users/${userId}/queues?pageSize=200`
		$.ajax({
			url: url,
			type: "GET",
			beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
			success: function (result) {
				console.log(result, "getQueuesByUser-success")
				result = result && result.entities?result.entities:[]														
				resolve(result);
			},
			error: function (request) {
				console.log("getQueuesByUser", "error -> url: " + url + ", detail: " + JSON.stringify(request));
				reject("getQueuesByUser-error -> " + JSON.stringify(request));

			}
		});
	});
}

function getWrapupCodesByQueue(token, queueId)
{
	return new Promise((resolve, reject) => {
		const url = `https://api.${config.environment}/api/v2/routing/queues/${queueId}/wrapupcodes?pageSize=200`
		$.ajax({
			url: url,
			type: "GET",
			timeout: 60000,
			beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
			success: function (result) {
				console.log(result, "getWrapupCodesByQueue-success")	
				const $wrapupCodesCtrl = $("#wrapupCodesCtrl");
				$wrapupCodesCtrl.empty()
				$wrapupCodesCtrl.append($("<option />").val('').text(''));
				$.each(result.entities, function() {
					$wrapupCodesCtrl.append($("<option />").val(this.id).text(this.name));
				});				
				resolve(result);
			},
			error: function (request) {
				console.log("getWrapupCodesByQueue", "error -> url: " + url + ", detail: " + JSON.stringify(request));
				reject("getWrapupCodesByQueue.error -> " + JSON.stringify(request));

			}
		});
	});
}

function setWrapupCode(token, conversationId, agentParticipantId, wrapupCode)
{
	const url = `https://api.${config.environment}/api/v2/conversations/calls/${conversationId}/participants/${agentParticipantId}`
	
	return new Promise((resolve, reject) => {
		$.ajax({
			url: url,
			type: "PATCH",
			data: JSON.stringify({ "wrapup": { "code": wrapupCode, "notes": "", "tags": [] } }),
			beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
			contentType: 'application/json; charset=utf-8',
			dataType: 'json',
			timeout: 60000,
			success: function (result) {
				console.log("update wrapup code - success", result)
				$.ajax({
					url: url,
					type: "PATCH",
					data: JSON.stringify({"state":"DISCONNECTED"}),
					beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
					contentType: 'application/json; charset=utf-8',
					dataType: 'json',
					success: function (result) {
						console.log("disconnect call - success", result)
						resolve(result)
					},				
					error: function (request) {
						console.log("disconnectCall", "error -> url: " + url + ", detail: " + JSON.stringify(request));
						reject("disconnectCall-error -> " + JSON.stringify(request));
	
					}
				});
			},				
			error: function (request) {
				console.log("setWrapupCode", "error -> url: " + url + ", detail: " + JSON.stringify(request));
				reject("setWrapupCode-error -> " + JSON.stringify(request));

			}
		});
	});
}

function getActiveInteraction(token) { 
	const url = "https://api." + config.environment + "/api/v2/conversations";

	return new Promise((resolve, reject) => {
		$.ajax({
			url: url,
			type: "GET",
			timeout: 60000,
			beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
			success: function (result) {
				let conversations = []
				if (result && result.entities && result.entities.length > 0) {							
					for (const conversation of result.entities){						
						conversations.push(conversation)
					}
					console.log(conversations, "getActiveInteraction-success")
				}
				resolve(conversations)				
			},
			error: function (request) {
				console.log("getActiveInteraction", "error -> url: " + url + ", detail: " + JSON.stringify(request))
				reject("getActiveInteraction-error -> " + JSON.stringify(request));  
			}
		});
	});
	
};	

function getActiveEmailInteraction(token) {
	const url = "https://api." + config.environment + "/api/v2/conversations/emails";

	return new Promise((resolve, reject) => {
		$.ajax({
			url: url,
			type: "GET",
			timeout: 60000,
			beforeSend: function (xhr) { xhr.setRequestHeader('Authorization', 'bearer ' + token); },
			success: function (result) {
				let conversations = []
				if (result && result.entities && result.entities.length > 0) {
					for (const conversation of result.entities) {
						conversations.push(conversation)
					}
					console.log(conversations, "getActiveEmailInteraction-success")
				}
				resolve(conversations)
			},
			error: function (request) {
				console.log("getActiveEmailInteraction", "error -> url: " + url + ", detail: " + JSON.stringify(request))
				reject("getActiveEmailInteraction-error -> " + JSON.stringify(request));
			}
		});
	});

};

const saveWrapupCodeAsync = async (conversation, attempt) => {
	if(!attempt){
		attempt = 1
	}	
	while (attempt <= config.maxAttempts) {
		try {
			await saveWrapupCode(conversation)
			return				
		} catch (error) {
			if(attempt == config.maxAttempts){
				throw new Error(error);
			}
			await sleep(3000)
		}			
		attempt++	
	}
}

function saveWrapupCode(conversation)
{
	const url = config.wrapupServiceUri
	return new Promise((resolve, reject) => {	
		$.ajax({
				url: url,
				type: "POST",
				contentType: "application/json",
				dataType: 'json',
				data: JSON.stringify(conversation),
				timeout: 60000,
				success: function (result){
					console.log(result, "saveWrapupCode-success")
					if (result.success) {
						resolve(result)
					}
					else {
						reject(result.status)
					}
											
				},
				error: function (request) {
					console.log("saveWrapupCode", `error -> url: ${url}, detail: ${JSON.stringify(request)}`);						
					reject("saveWrapupCode.error -> " + JSON.stringify(request))
				}
		});	
	})	
}

function hideMessages() {
	$("#warningMessage").hide()
	$("#errorMessage").hide()
}

function showMessage(type, text, ms = 0) {
	hideMessages()
	switch (type) {
		case "error":
			$("#errorMessage").text(text)
			$("#errorMessage").show()
			break;
		case "warning":
			$("#warningMessage").text(text)
			$("#warningMessage").show()
			break;
	
		default:
			break;
	}
	if(ms > 0){
		setTimeout(hideMessages, ms);
	}
	
}

const sleep = (ms) => {
	return new Promise(resolve => setTimeout(resolve, ms));
}

function updateCurrentQueueText(text) {
	$("#currentQueue").text(`Active queue: ${text}`)
}

	



	

	
	
	