function filter(baseUrl) {
    var filterValue = document.getElementById("filterInput").value;
    if (filterValue == null || filterValue == "" || baseUrl == null || baseUrl == "") {
        return;
    }
    var url = baseUrl.replace("$replaceMe$", filterValue);
    window.location.href = url;
}
