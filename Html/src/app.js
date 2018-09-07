var app = {
    menuMap: [],
    tables: [],
    tableIndex: {},
    currentTable: null,
    table: null,
    $tableContainer: $('#table'),
    $table: $('#table>table'),
    $tableToggle: $('#table-toggle'),
    $tfoot: $('#table>table>tfoot'),
    tableAllExpanded: false
}

 
 
instantiateTables();
app.tables[0].show();