
function save() {

	textBox = document.getElementById("textBox");
	callback = function(response) {
		if(response == "Done!") {
			location.href = "saved";
		}
	}
	derp = document.getElementById("textBox");
	derp = derp.value;                //only slightly copy pasta here because regular expressions.
	derp = derp.replace(/\n/g, ' ').replace(/ +/g, " ").trim();
	req = new request("Parents.txt", "SAVE", null, this.callback, [["numbers", derp]]);
}

function fillBox(response) {
	response = response.split("\r\n").join(" ");
	response = response.split("\n").join(" ");
	console.log(response);
	textBox = document.getElementById("textBox");
	
	textBox.value += "\n\n\n";
	textBox.value += response;
}

listRequest = new request("Parents.txt", "GET", null, fillBox, null);
