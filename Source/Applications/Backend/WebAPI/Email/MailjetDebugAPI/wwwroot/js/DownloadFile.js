function DownloadFile(filename, mimeType, content) {
	var link = document.createElement('a');
	link.download = filename;
	link.href = "data:" + mimeType + ";base64," + content;
	document.body.appendChild(link);
	link.click();
	document.body.removeChild(link);
}
