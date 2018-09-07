var reportApp = {
    menuMap: [],
	pages: [],
	currentPageId: -1,//means no page has been shown
    tables: [],
    tableIndex: {},
    currentTable: null,
    table: null,
    $tableContainer: $('#table'),
    $table: $('#table>table'),
    $tableToggle: $('#table-toggle'),
    tableAllExpanded: false,
	trace:''
}

 
initMenuMap();
$('#group-menu').drawMenu();
showPage(0);