(function () {
    var $tableName = $('#table-name');
    var $tableDescription = $('#table-description');
    var $afterTable = $('#after-table');
    var $tableSummary = $('#table-summary');
    $tableDescription.add($tableSummary).on('click', 'a[tid]', function () {
        var tid = $(this).attr('tid');
        app.tables[app.tableIndex[tid]].show();
    });
    window.Table = function (raw) {
        this.id = raw.tableId;
        this.name = raw.displayTitle;
        this.tag = raw.tag;
        this.description = raw.description;
        this.type = raw.layout;
        this.bgColorExpand = raw.bgColorExpand;
        this.expandEnabled = raw.expandEnabled;
        this.allowSort = raw.sortEnabled;
        this.allowFilter = raw.filterEnabled;
        this.summary = raw.summary;
        this.expandedExtraAsHtml = raw.expandedExtraAsHtml;
        this.tableTitle = raw.tableTitle;
        this.afterTable = raw.afterTable;
        this.limitTableWidth = raw.limitTableWidth;


        if (!!raw.pageLength) this.pageLength = raw.pageLength;
        else
            this.pageLength = 20;

        this.splitDisabled = !!raw.splitDisabled;
        this.verticalExcludeColumns = raw.verticalExcludeColumns;
        this.verticalBigColumns = raw.verticalBigColumns;
        this.verticalLongColumns = raw.verticalLongColumns;
        this.columnFilterEnabled = raw.columnFilterEnabled;

        this.columnDefs = raw.columnDefs;
        this.columns = [];
        this.data = [];

        this.initColumns(raw);
        this.initData(raw);

        this.dataTableParams = {
            dom: 'Blfrtip',
            buttons: ['copy', 'excel'],
            data: this.data,
            columns: this.columns,
            lengthMenu: [
                [20, 50, 100,200, -1],
                [20, 50, 100,200, 'All']
            ],
            searching: this.allowFilter,
            ordering: this.allowSort,
            order: [],
            autoWidth: false,
            columnDefs: this.columnDefs,
            pageLength: this.pageLength
           


        };
    }

    Table.prototype.show = function () {
        var me = this;
        if (app.currentTable) {
            app.currentTable.hide();
        }
        app.currentTable = this;
        //$tableName.text(this.name);
        $tableName.html(this.name);
        if (this.description) {
            $tableDescription.html(this.description);
        }

        if (this.afterTable) {
            $afterTable.html('<div>{0}</div>'.format(this.afterTable));
        }
        else $afterTable.html('<div>{0}</div>'.format(''));
        if (this.summary) {
            if (this.summary !== '') $tableSummary.html('<h3>Summary</h3><div>{0}</div>'.format(this.summary));
        }
        app.$tableContainer.addClass(this.type);

        if (this.limitTableWidth)
        {
            var tableWidth = 0;
            this.dataTableParams.columns.forEach(function (column) {
                tableWidth = tableWidth + Math.ceil(column.width);
            });
            //search filter is 500 width, so set it min as 600
            if (tableWidth < 600) tableWidth = 600;
            app.$tableContainer.css({
                "width": tableWidth + "px" 
               // "backgroundColor": "red",
               // "border": "2px"
            });

        }

        //If page length change, need to update the size change
        app.$tableContainer.on('length.dt', function () {
            SizeCouldChangeUsingTimeout();
        });

        //If page change, need to update the size change as well
        app.$tableContainer.on('page.dt', function () {
            SizeCouldChangeUsingTimeout();
        });

        

        if (this.type !== 'container') {
            if (app.table) {
                this.dataTableParams.destroy = true;
                this.dataTableParams.empty = true;
            }
            var footHtml = '<tr>';
            this.dataTableParams.columns.forEach(function (column) {
                //footHtml += '<td><input type="text" placeholder="Search ' + column.title + '"/></td>';
                footHtml += '<td style="text-align:left;"><input type="text" placeholder="Search ' + column.title + '" style="width:' + Math.floor(column.width * 0.9) + 'px;"  /></td>';
            });
            footHtml += '</tr>';
         
            //Only show it if it is enabled.
            if (this.columnFilterEnabled)  app.$tfoot.html(footHtml);

            app.table = app.$table.DataTable(this.dataTableParams);

            app.table.columns().every(function () {
                    var that = this;
                    $('input', this.footer()).on('keyup change', function () {
                        if (that.search() !== this.value) {
                            that.search(this.value).draw();
                            SizeCouldChangeUsingTimeout();
                        }
                    });
                });
           
            
        } else {
            app.$tableContainer.html('');
            this.data.forEach(function (row, index) {
                var content = '<div class="line">';
                content += '<div>' + createVerticalTable(me.columns, row, me) + '</div>';
                content += '<div>' + me.getExtraTable(index) + '</div>';
                content += '</div>';
                app.$tableContainer.append(content);
            });
        }
        app.tableAllExpanded = false;
        if (this.type === 'both' && !!this.expandEnabled) {
            $('.dataTables_length').append('&nbsp;&nbsp;&nbsp;<a id="toggle-all" href="javascript:void(0)"><i class="fa fa-plus-square-o" aria-hidden="true"></i> Expand All</a>');
        }

        if (this.tableTitle != '' && !!this.tableTitle) $('.dataTables_length').append("<span style='margin-left:80px'>&nbsp;</span>" + this.tableTitle);
    };

    Table.prototype.getExtraTable = function (rowIndex, rawHtml) {

        if (rowIndex != 0 && (rowIndex == null || !rowIndex)) return '';
        if (this.data[rowIndex].length === this.columns.length + 1) {
            var columns = [];
            var row = [];
            for (var key in this.data[rowIndex][this.columns.length]) {
                columns.push({ title: key });
                row.push(this.data[rowIndex][this.columns.length][key]);
            }
            return createVerticalTableForExtra(columns, row, rawHtml, null);
        } else {
            return '';
        }
    };



    Table.prototype.hide = function () {
        $tableName.html('');
        $tableDescription.html('');
        $tableSummary.html('');
        $afterTable.html('');

        if (this.type !== 'container') {
            app.table.destroy();
            app.$table.empty();

            app.$table.append('<tfoot></tfoot>');
            app.$tfoot = app.$table.find('tfoot');
           
        } else {
            app.$tableContainer.html('<table class="display cell-border compact"><tfoot></tfoot></table>');
            app.$table = app.$tableContainer.find('table');
            app.$tfoot = app.$tableContainer.find('table>tfoot');
        }
        app.$tableContainer.removeClass(this.type);
    };

    function GetMaxWidth(header) {
        var w = 0;
        header.forEach(function (col, index) {
            w = Math.max(w, col.title.length);

        });
        return w;
    }

    function GetMaxWidthData(header) {
        var w = 0;
        header.forEach(function (col, index) {
            w = Math.max(w, col.chars);

        });
        return w;
    }

    Table.prototype.initColumns = function (raw) {
        var me = this;
        if (this.type === 'vertical') {
            w = GetMaxWidth(raw.header);
            wdata = GetMaxWidthData(raw.header);

            this.columns.push({
                title: 'Key'
				, width: w * 10
            });
            this.columns.push({
                title: 'Value'
				 , width: wdata * 10
            });
        } else {
            var idx = 0;
            raw.header.forEach(function (col, index) {



                var item = {
                    title: col.title,
                    width: col.chars * 10
                };
                me.columns.push(item);
            });
        }
    };

    Table.prototype.initData = function (raw) {
        var me = this;
        if (this.type === 'vertical') {
            raw.header.forEach(function (key, index) {
                me.data.push([key.title, raw.data[0][index]]);
            });
        } else {
            this.data = raw.data;
        }
    };
})();


function datatableSizeCouldChange() {
    var da = document.getElementById("data-area");

    //set this so I can get the correct scrollHeight. otherwise once you set it to larger value, it won't shrink.
    da.style.height = Math.floor(window.screen.height*1.2) + "px"; //times 1.2, to avoid sudden scroll up. still occur, but less

    var maxHeight = Math.max(da.scrollHeight, da.offsetHeight, da.clientHeight);
    var maxWidth = Math.max(da.scrollWidth, da.offsetWidth);



    da.style.width = maxWidth + "px";
    da.style.height = maxHeight + "px";


    //alert("scrollWidth:"+da.scrollWidth+ " offsetWidth:"+da.offsetWidth )
    if (parent.postMessage) {
        parent.postMessage({ "maxWidth": maxWidth, "maxHeight": maxHeight }, "*");
    }


}


//add function to post message when page length is change, when row is click etc
//execute it with timeout because layout change may need sometime to finish 


var myTimeoutObj = null;
function SizeCouldChangeUsingTimeout() {
    //clear it, to avoid the timeout occurs frequently, we just need one timeout to be fired.
    if (!!myTimeoutObj) clearTimeout(myTimeoutObj)
    myTimeoutObj = setTimeout(function () { datatableSizeCouldChange(); }, 100);

}

function createVerticalTableForExtra(columns, row, rawHtml) {
    var table = '';
    if (!rawHtml) table = '<table class="vt">';
    else table = '<table class="vt" style="margin:0px !important;">'; //overwrite the vt class 

    var tableColLength = Math.ceil(columns.length / 16) * 2;
    var tableRowLength = Math.ceil(columns.length / (tableColLength / 2));
    var key, value, dataIndex;
    for (var i = 0; i < tableRowLength; i++) {
        //specify class=inner-line, to avoid the click event. In instantiateTables there is a click event on tr object
        table += '<tr class="inner-line">';
        for (var j = 0; j < tableColLength; j += 2) {
            dataIndex = j ? (j - 1) * tableRowLength + i : i;
            if (dataIndex >= columns.length) {
                break;
            }
            if (!rawHtml)
                table += '<td style="cursor:default;">{0}</td><td style="cursor:default;min-width:80px;max-width:600px;word-wrap:break-word;word-break:break-all;">{1}</td>'.format(columns[dataIndex].title.encodeHTML(), row[dataIndex]);
            else if (row[dataIndex] != "") //if i want to display it as html 
                table += '<td style="border:solid 0 red !important;background-color:lightYellow;margin:0;padding:12px;cursor:default;min-width:80px;overflow:auto;word-wrap:break-word;">{0}</td>'.format(row[dataIndex]);
        }
        table += '</tr>';
    }
    return table + '</table>';
};

function GetBrowser() {

    // Opera 8.0+
    var isOpera = (!!window.opr && !!opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0;

    // Firefox 1.0+
    var isFirefox = typeof InstallTrigger !== 'undefined';

    // Safari 3.0+ "[object HTMLElementConstructor]" 
    var isSafari = /constructor/i.test(window.HTMLElement) || (function (p) { return p.toString() === "[object SafariRemoteNotification]"; })(!window['safari'] || safari.pushNotification);

    // Internet Explorer 6-11
    var isIE = /*@cc_on!@*/false || !!document.documentMode;

    // Edge 20+
    var isEdge = !isIE && !!window.StyleMedia;

    // Chrome 1+
    var isChrome = !!window.chrome && !!window.chrome.webstore;

    // Blink engine detection
    var isBlink = (isChrome || isOpera) && !!window.CSS;

    var output = 'Detecting browsers by ducktyping:<hr>';
    output += 'isFirefox: ' + isFirefox + '<br>';
    output += 'isChrome: ' + isChrome + '<br>';
    output += 'isSafari: ' + isSafari + '<br>';
    output += 'isOpera: ' + isOpera + '<br>';
    output += 'isIE: ' + isIE + '<br>';
    output += 'isEdge: ' + isEdge + '<br>';

    return output;
}


function createVerticalTable(columns, row, me) {
    if (!row) return '';
    var MaxTDWidth = 1600;
    var MinTDWidth = 400;
    var MaxTDHeight = 800;
    var MinTDHeight = 400;
    //chrome					  
    //chrome
    var isChrome = !!window.chrome && !!window.chrome.webstore;
    var isEdge = !isIE && !!window.StyleMedia;
    var isIE = /*@cc_on!@*/false || !!document.documentMode;


    if (isChrome) { MinTDWidth = Math.max(200, Math.floor(0.2 * window.screen.width)); MaxTDWidth = Math.max(Math.floor(0.5 * window.screen.width), MinTDWidth); }
    else if (isEdge || isIE) { MinTDWidth = Math.max(200, Math.floor(0.1 * window.innerWidth)); MaxTDWidth = Math.max(Math.floor(0.5 * window.innerWidth), MinTDWidth); }

    // alert(MaxTDWidth);


    if (isChrome) { MinTDHeight = Math.max(400, Math.floor(0.3 * window.screen.height)); MaxTDHeight = Math.max(Math.floor(0.5 * window.screen.height), MinTDHeight); }
    else if (isEdge || isIE) { MinTDHeight = Math.max(400, Math.floor(0.3 * window.innerHeight)); MaxTDHeight = Math.max(Math.floor(0.5 * window.innerHeight), MinTDHeight); }

    //if no big column, then as wide as possible
    if ((me && me.verticalBigColumns && me.verticalBigColumns.length == 0)
        || (me && !!!me.verticalBigColumns)) {
        if (isChrome) { MinTDWidth = Math.max(400, Math.floor(0.3 * window.screen.width)); MaxTDWidth = Math.max(Math.floor(0.7 * window.screen.width), MinTDWidth); }
        else if (isEdge || isIE) { MinTDWidth = Math.max(400, Math.floor(0.3 * window.innerWidth)); MaxTDWidth = Math.max(Math.floor(0.7 * window.innerWidth), MinTDWidth); }

    }
    // alert(MaxTDWidth);
    var MaxTDWidth_outer = MaxTDWidth + 50;

    var specialColumnCount = 0;
    uniqueSpecialColumns = [];


    //take care of those specical columns	
    if (me) {


        if (me.verticalExcludeColumns) {
            for (var u = 0; u < me.verticalExcludeColumns.length; u++)
                if (uniqueSpecialColumns.indexOf(me.verticalExcludeColumns[u]) < 0) uniqueSpecialColumns.push(me.verticalExcludeColumns[u]);

        }

        if (me.verticalBigColumns) {
            for (var u = 0; u < me.verticalBigColumns.length; u++)
                if (uniqueSpecialColumns.indexOf(me.verticalBigColumns[u]) < 0) uniqueSpecialColumns.push(me.verticalBigColumns[u]);

        }
        if (me.verticalLongColumns) {
            for (var u = 0; u < me.verticalLongColumns.length; u++)
                if (uniqueSpecialColumns.indexOf(me.verticalLongColumns[u]) < 0) uniqueSpecialColumns.push(me.verticalLongColumns[u]);

        }

    }

    specialColumnCount = uniqueSpecialColumns.length;

    //need to exclude the special columns
    var len = columns.length - specialColumnCount;

    var tableColLength = Math.ceil(len / 16) * 2; //caculate how many columns in vertical I should have. Each column have 16 rows.
    if (me.splitDisabled == true) tableColLength = 2;
    var tableRowLength = Math.ceil(len / (tableColLength / 2)); //How many actual rows I should have in vertical view
    var key, value, dataIndex, delta;
    delta = 0;

    var table = '';
    //Ignore the min width
    //table = '<table class="vt" style="min-width:{0}px;max-width:{1}px">'.format(MinTDWidth, MaxTDWidth);
    table = '<table class="vt" style="max-width:{0}px;border:solid 1px grey;">'.format(MaxTDWidth);


    for (var i = 0; i < tableRowLength; i++) {
        table += '<tr class="inner-line">';
        for (var j = 0; j < tableColLength; j += 2) {
            dataIndex = j ? (j - 1) * tableRowLength + i : i;

            dataIndex = dataIndex + delta;
            //if it is special column I need to advance.

            var advance = 0;

            if (me) {
                var idx = dataIndex;

                while (idx < columns.length) //advance until i find one column which is not special
                {
                    if (me.verticalExcludeColumns && (me.verticalExcludeColumns.indexOf(idx) >= 0)) { advance = advance + 1; }
                    else if (me.verticalBigColumns && (me.verticalBigColumns.indexOf(idx) >= 0)) { advance = advance + 1; }
                    else if (me.verticalLongColumns && (me.verticalLongColumns.indexOf(idx) >= 0)) { advance = advance + 1; }
                    else break;//if this column is not special, then break and use it.
                    idx++;

                } //end while

            }

            dataIndex = dataIndex + advance;
            delta = delta + advance



            if (dataIndex >= columns.length) {
                break;
            }
            if(me.tag==1) //summary page, don't show the column header for RuleResult.
                table += '<td style="padding:12px;background-color:lightyellow;cursor:default;min-width:80px;overflow:auto;word-wrap:break-word;word-break:normal;">{0}</td>'.format( row[dataIndex]);
            else table += '<td style="cursor:default;">{0}</td><td style="cursor:default;min-width:80px;overflow:auto;word-wrap:break-word;word-break:break-all;">{1}</td>'.format(columns[dataIndex].title.encodeHTML(), row[dataIndex]);



        }
        table += '</tr>';



    }//end for




    table = table + '</table>';


    //now add the long column.
    var longTR = "";

    if (me && me.verticalLongColumns && me.verticalLongColumns.length > 0) {

        for (var k = 0; k < me.verticalLongColumns.length; k++) {

            var longIdx = me.verticalLongColumns[k];
            if (longIdx >= columns.length) break;
            if (row[longIdx] != "") {
                //  longTR+= '<tr><td>{0}</td></tr><tr><td style="color:blue;padding:6px;cursor:default;word-wrap:break-word;">{1}</td></tr>'.format(columns[longIdx].title,row[longIdx]);
                longTR += '<tr class="inner-line"><td>{0}</td></tr><tr class="inner-line"><td><div style="background:white;color:blue;padding:6px;cursor:default;word-wrap:break-word;word-break:normal;max-width:{1}px;max-height:{2}px;overflow:auto;">{3}</div></td></tr>'.format(columns[longIdx].title, MaxTDWidth, MaxTDHeight, row[longIdx]);
            }
        }

        if (longTR != "") {
            longTR = '<table>{0}</table>'.format(longTR);
            //    table='<table style="min-width:{0}px;max-width:{1}px;overflow:auto" class="vt"><tr><td style="cursor:default;">{2}</td></tr><tr><td style="">{3}</td></tr></table>'.format(
            //   MinTDWidth,MaxTDWidth,table,longTR);

            table = '<table style="min-width:{0}px;max-width:{1}px; overflow:auto;border:solid 1px grey;background:white;" class="vt"><tr class="inner-line"><td style="cursor:default;">{2}</td></tr><tr class="inner-line"><td style="">{3}</td></tr></table>'.format(
            MinTDWidth, MaxTDWidth_outer, table, longTR);

        }

    }









    //Now it is time to process special columns

    var bigTD = "";

    if (me && me.verticalBigColumns && me.verticalBigColumns.length > 0) {

        for (var k = 0; k < me.verticalBigColumns.length; k++) {

            var bigIdx = me.verticalBigColumns[k];
            if (bigIdx >= columns.length) break;
            //add white-space:nowrap to disable line feed
            bigTD += '<tr class="inner-line"><td>{0}</td></tr><tr class="inner-line"></tr><tr class="inner-line"><td style="color:blue;padding:2px;cursor:default;max-width:{1}px;overflow:auto;white-space:nowrap;"><pre>{2}</pre></td></tr>'.format(columns[bigIdx].title, MaxTDWidth, row[bigIdx]);


        }

        bigTD = '<td style="vertical-align:top;cursor:default;"><table class="vt" style="border:solid 1px grey;">{0}</table></td>'.format(bigTD);

    }



    //Now time to add extract column (nested data)

    var nestedStr = "";

    if (row.length === columns.length + 1) {
        var last = columns.length;
        for (var key in row[last]) {
            nestedStr += '<tr class="inner-line"><td>{0}</td></tr><tr class="inner-line"></tr><tr class="inner-line"><td style="color:blue;padding:6px;cursor:default;max-width:{1}px;overflow:auto;">{2}</td></tr>'.format(key, MaxTDWidth, row[last][key]);
        }
        if (nestedStr != "") nestedStr = '<td style="vertical-align:top;cursor:default;"><table class="vt">{0}</table></td>'.format(nestedStr);
    }


    var result = '<table><tr class="inner-line" style="background-color:{0};"><td style="vertical-align:top;cursor:default;">{1}</td>{2}{3}</tr></table>'.format(app.currentTable.bgColorExpand,table, bigTD, nestedStr);

    return result;




};


function instantiateTables() {
    var tables = app.tables;
    var tableIndex = app.tableIndex;
    SQLDumpDataTables.forEach(function (table) {
        var tableId = table.tableId;
        tables.push(new Table(table));
        tableIndex[tableId] = tables.length - 1;
    });
    $('#data-area').on('click', '#toggle-all', function () {
        if (app.currentTable.type !== 'both' || app.currentTable.expandEnabled == false) {
            return;
        } else {
            app.$tableContainer.find(app.tableAllExpanded ? 'tr.expanded' : 'tr:not(.expanded)').click();
            $(this).html(app.tableAllExpanded ? '<i class="fa fa-plus-square-o" aria-hidden="true"></i> Expand All' : '<i class="fa fa-minus-square-o" aria-hidden="true"></i> Collapse All');
            app.tableAllExpanded = !app.tableAllExpanded;
        }
    });
    app.$tableContainer.on('click', 'tbody>tr[class !="inner-line" ]', function () {
        if (app.currentTable.type !== 'both' || app.currentTable.expandEnabled == false) {
            return;
        }

        var row = app.table.row(this);
        var rowData = row.data();

        //use this one to avoid empty click effect, especially those inner table created by SQLDumpViewer
        if (!rowData) return;


        var $tr = $(this);
        if ($tr.hasClass('expanded')) {
            $tr.removeClass('expanded').next().remove();
            //update parent size could change
            SizeCouldChangeUsingTimeout();
            return;
        }
        $tr.addClass('expanded');


        //some columes are not visible. so if set colspan to columes length, the UI will shrink when you click the row.
        //在undifined和null时，用一个感叹号返回的都是true
        var specialColumns = 0;
        if (!!app.currentTable.columnDefs) //if table does has columnDefs
            if (app.currentTable.columnDefs[0].targets) specialColumns = app.currentTable.columnDefs[0].targets.length;
        var $td = $('<td colspan="{0}" class="table-td" style="padding:2px;cursor:default;background-color:#AAAAAA !important"></td>'.format((app.currentTable.columns.length - specialColumns)));

        var $newTr = $('<tr></tr>');
        $newTr.append($td);
        $tr.after($td);
        if (!app.currentTable.expandedExtraAsHtml) {
            var s1 = createVerticalTable(app.currentTable.columns, rowData, app.currentTable);
            // var s2=app.currentTable.getExtraTable(row.index());
            $td.html(s1);
        }


        else $td.html(app.currentTable.getExtraTable(row.index(), true));//just display extra table, ignore other normal columns

        //use click something. so I need to update size to parent, to avoid scrollbar
        SizeCouldChangeUsingTimeout();

    });
}