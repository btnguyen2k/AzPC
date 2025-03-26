function Table2Excel(tableId, filename) {
    var table = document.getElementById(tableId);
    if (!table) {
        alert("Table with ID " + tableId + " not found.");
        return;
    }
    var htmlBase64 = btoa(table.outerHTML);
    var link = document.createElement('a');
    link.download = filename || 'table.xls';
    link.href = 'data:application/vnd.ms-excel;base64,' + htmlBase64;
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}
