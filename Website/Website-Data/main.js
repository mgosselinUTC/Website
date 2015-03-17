function request(path, method, data, callback, headers) {
	xhr = new XMLHttpRequest();
	xhr.open(method, path, true); 
	xhr.onreadystatechange = function () {						
		if (xhr.readyState==4 && xhr.status==200) {
			console.log("wasdsdat");

			callback(xhr.response);
		}
	}
	//add dem headers yo
	if(headers != null) {
		for(var i = 0; i < headers.length; i++) {
			xhr.setRequestHeader(headers[i][0], headers[i][1]);
		}
	}
	xhr.send(data);
}

function send() {


	var textBox = document.getElementById('textBox');
	var message = textBox.value;
	console.log(message);

	req = new request("Parents.txt", "SEND", null, function(response){
	
		if(response == "Done!") {
			location.href = "sent";
		}
	
	}, [["message", message]]);
	
	// my totally awesome web server will take all send requests, and text the 
	// body of the HTTP Request to the phone numbers listed in the given filename
	// hosted serverside.
}
