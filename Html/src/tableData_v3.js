
//Add size control global

function datatableSizeCouldChange() {
    var da = document.getElementById("data-area");

    //set this so I can get the correct scrollHeight. otherwise once you set it to larger value, it won't shrink.
    da.style.height = Math.floor(window.screen.height * 1.2) + "px"; //times 1.2, to avoid sudden scroll up. still occur, but less

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




var tableDataModule = (function (mdl) {

    //to show the page content
    mdl.showPage = function () {
        $("#page-title").html(pageData.pageTitle);
        $("#page-description").html(pageData.pageDescription+"<br");
        $("#page-table").html(pageData.pageContent);
        $("#page-summary").html(pageData.pageSummary);


    }


    function uuidv4() {
        return 'xxxxxxxx_xxxx_4xxx_yxxx_xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }
    function GetTableObj(id) {
        var len = pageData.dtables.length;
        for (var i = 0; i < len; i++) {
            if (pageData.dtables[i].tableId == id) return pageData.dtables[i];
        }

        return null;
    }
    // mdl.ShowTable = function (tableId) {


    // var parentContainerName="page-table";
    // var myTableDiv = document.getElementById(parentContainerName);	
    // mdl.ShowTableInDiv(myTableDiv,tableId);
    // };
    mdl.ShowTable = function (parentContainerName, tableId) {

        var tableObj = GetTableObj(tableId);
        if (!tableObj) return;

        tableObj.AllRowsExpanded = false;
        // var parentContainerName="page-table";
        var myTableDiv = document.getElementById(parentContainerName);
        if (!myTableDiv) return;
        //var myTableDiv=divObj;
        ////////////////////////////////////////////////////////		
        ////////////////////////////////////////////////////////
        //Init colums

        var cols = InitColumns(tableObj);

        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        //Create parement Div


        var $myTableDiv = $(myTableDiv);
        var $table = $("<table class='display cell-border compact'><tfoot></tfoot></table>",
		{
		    id: tableObj.tableId//'tableId' + uuidv4()
		}
		).appendTo($myTableDiv);

        $table.addClass("both");



        //Create table description and summary section
        if (!!tableObj.tableDescription && tableObj.tableDescription != "") {
            var descSpan = document.createElement('DIV');
            $(descSpan).css({
                "padding-bottom": "10px", //set this so there is some room on top of the table
                //  "backgroundColor": "red",

            });

            $table.before(descSpan);
            $(descSpan).html(tableObj.tableDescription);
        }
        if (!!tableObj.tableSummary && tableObj.tableSummary != "") {
            var sumSpan = document.createElement('SPAN');
            $table.after(sumSpan);
            $(sumSpan).html(tableObj.tableSummary);
        }
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        //params
        var dataTableParams = {
            dom: 'Blfrtip',
            buttons: ['copy', 'excel'],
            data: tableObj.data,
            //columns: [ { title: "col1", width: 12 }, { title: "col2", width: 590 }, { title: "more", width: 10 }],
            columns: cols,
            lengthMenu: [
                [20, 50, 100, 200, -1],
                [20, 50, 100, 200, 'All']
            ],
            searching: tableObj.filterEnabled,
            ordering: tableObj.sortEnabled,
            order: [],
            autoWidth: false,
            columnDefs: tableObj.columnDefs,
            pageLength: tableObj.pageLength



        };

        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////

        if (tableObj.limitTableWidth) {
            var tableWidth = 0;
            dataTableParams.columns.forEach(function (column) {
                tableWidth = tableWidth + Math.ceil(column.width);
            });
            //search filter is 500 width, so set it min as 600
            if (tableWidth < 600) tableWidth = 600;
            $table.css({
                "width": tableWidth + "px",
                "text-align": "left",
                "margin": 0, //set this to force table left align
                //  "backgroundColor": "red",
                // "border": "2px"
            });

        }
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        //Add foot to support column level filter
        if (tableObj.columnFilterEnabled) {
            var footHtml = '<tr>';
            dataTableParams.columns.forEach(function (column) {
                //footHtml += '<td><input type="text" placeholder="Search ' + column.title + '"/></td>';
                footHtml += '<td style="text-align:left;"><input type="text" placeholder="Search ' + column.title + '" style="width:' + Math.floor(column.width * 0.9) + 'px;"  /></td>';
            });
            footHtml += '</tr>';

            //Only show it if it is enabled.
            $table.find("tfoot").html(footHtml);

        }



        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        //Time to init the datatable

        var dtable = $table.DataTable(dataTableParams);

        if (!!tableObj.expandEnabled) {
            $myTableDiv.find('.dataTables_length').append('&nbsp;&nbsp;&nbsp;<a id="toggle-all" href="javascript:void(0)"><i class="fa fa-plus-square-o" aria-hidden="true"></i> Expand All</a>');
        }
        //Table title
        if (!!tableObj.tableTitle && tableObj.tableTitle != '') {

            var t = $myTableDiv.find(".dataTables_length").append("<span style='margin-left:80px'>&nbsp;</span>" + tableObj.tableTitle);


        }



        //set the search input position, the paging information

        $myTableDiv.find(".dataTables_filter").css({
            "display": "inline-block",
            "float": "none",
            "margin-left": "40px",
            "text-align": "left",
            //  "backgroundColor": "red",
            // "border": "2px"
        });


        $myTableDiv.find(".dataTables_paginate").css({
            "display": "inline-block",
            "float": "none",
            "text-align": "left",
            //  "backgroundColor": "red",
            // "border": "2px"
        });

        ///Add column level filter function

        if (tableObj.columnFilterEnabled) {
            dtable.columns().every(function () {
                var that = this;
                $('input', this.footer()).on('keyup change', function () {
                    if (that.search() !== this.value) {
                        that.search(this.value).draw();
                        SizeCouldChangeUsingTimeout();
                    }
                });
            });
        }



        //If page length change, need to update the size change
        $table.on('length.dt', function () {
            SizeCouldChangeUsingTimeout();
        });

        //If page change, need to update the size change as well
        $table.on('page.dt', function () {
            SizeCouldChangeUsingTimeout();
        });

        //Expand all or collapse all
        if (!!tableObj.expandEnabled) {
            $myTableDiv.on('click', '#toggle-all', function () {

                $table.find(tableObj.AllRowsExpanded ? 'tr.expanded' : 'tr:not(.expanded)').click();
                $(this).html(
                tableObj.AllRowsExpanded ?
                '<i class="fa fa-plus-square-o" aria-hidden="true"></i> Expand All'
                : '<i class="fa fa-minus-square-o" aria-hidden="true"></i> Collapse All');
                tableObj.AllRowsExpanded = !tableObj.AllRowsExpanded;

            });
        }
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////
        // Add event listener for opening and closing details

        if (!!tableObj.expandEnabled) {
            $table.on('click', 'tbody>tr[class !="expanded-child-tr" ]', function () {
                //	$table.on('click', 'td.CanExpandChildRow', function () {
                //t.on('click', 'tbody>td.CanExpandChildRow', function () {
                var tr = $(this);// $(this).closest('tr');
                var row = dtable.row(tr);

                var data = row.data();
                //use this one to avoid empty click effect, especially those inner table created by SQLDumpViewer
                if (!data) return;

                if (row.child.isShown()) {
                    // This row is already open - close it
                    row.child.hide();
                    tr.removeClass('expanded');
                    SizeCouldChangeUsingTimeout();
                }
                else {
                    // Open this row
                    // row.child( format(row.data()) ).show();


                    row.child(createVerticalTable(dataTableParams.columns, data, tableObj)).show();

                    //Add background color to the expanded tr 
                    var child_tr = tr.next();
                    $(child_tr).css({ "background-color": "#AAAAAA", "cursor": "default" });
                    $(child_tr).addClass("expanded-child-tr");


                    tr.addClass('expanded');

                    SizeCouldChangeUsingTimeout();
                }
            });
        }
        ////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////




    }; //ShowTable


    function format(d) {
        // `d` is the original data object for the row
        return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
            '<tr>' +
                '<td>Id:</td>' +
                '<td>' + d[0] + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td>Title:</td>' +
                '<td>' + d[1] + '</td>' +
            '</tr>' +
            '<tr>' +
                '<td>Extra info:</td>' +
                '<td>' + d[2] + '</td>' +
            '</tr>' +
        '</table>';
    };


    function InitColumns(raw) {
        var columns = [];

        var idx = 0;
        raw.header.forEach(function (col, index) {


            if (!!raw.expandEnabled) {
                var item = {
                    title: col.title,
                    width: col.chars * 10,
                    className: "CanExpandChildRow"
                };
                columns.push(item);
            }

            else // not expandable
            {
                var item = {
                    title: col.title,
                    width: col.chars * 10

                };
                columns.push(item);


            }
        });

        return columns;
    };



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
        else if (isEdge || isIE) { MinTDWidth = Math.max(200, Math.floor(0.1 * window.innerWidth)); MaxTDWidth = Math.max(Math.floor(0.5 * window.screen.width), MinTDWidth); }

        // alert(MaxTDWidth);


        if (isChrome) { MinTDHeight = Math.max(400, Math.floor(0.3 * window.screen.height)); MaxTDHeight = Math.max(Math.floor(0.5 * window.screen.height), MinTDHeight); }
        else if (isEdge || isIE) { MinTDHeight = Math.max(400, Math.floor(0.3 * window.innerHeight)); MaxTDHeight = Math.max(Math.floor(0.5 * window.innerHeight), MinTDHeight); }

        //if no big column, then as wide as possible
        if ((me && me.verticalBigColumns && me.verticalBigColumns.length == 0)
            || (me && !!!me.verticalBigColumns)) {
            if (isChrome) { MinTDWidth = Math.max(400, Math.floor(0.3 * window.screen.width)); MaxTDWidth = Math.max(Math.floor(0.7 * window.screen.width), MinTDWidth); }
            else if (isEdge || isIE) { MinTDWidth = Math.max(400, Math.floor(0.3 * window.innerWidth)); MaxTDWidth = Math.max(Math.floor(0.7 * window.screen.width), MinTDWidth); }

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
                if (me.tag == 1) //summary page, don't show the column header for RuleResult.
                    table += '<td style="padding:12px;background-color:lightyellow;cursor:default;min-width:80px;overflow:auto;word-wrap:break-word;word-break:normal;">{0}</td>'.format(row[dataIndex]);
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


        var result = '<table><tr class="inner-line" style="background-color:{0};"><td style="vertical-align:top;cursor:default;">{1}</td>{2}{3}</tr></table>'.format(me.bgColorExpand, table, bigTD, nestedStr);

        return result;




    };











    //...

    return mdl;

})(window.tableDataModule || {});

