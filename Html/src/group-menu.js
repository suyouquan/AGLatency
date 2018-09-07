  
// menuMap = [{
//     groupName: 'group-name',
//     pages: [{
//         id:xx// unique id of each page
//         src: 'xxx.html', //html filename
//         pageName: 'pageName' //menuTitle
//         pageDivObj:  //the DIV object that contains the iframe
//         scrollY: $window.scrollTop() //the position of the page 
//         scrollX:,
//         width:100,
//         height:100
//     }, ...]
// }, ...]

//Function to create the Div for the iframe dynamically
function createDivIframe(id, pageSrc) {

    var $div = $("<div>",
	{
	    id: id, name: pageSrc
	});
    $div.css({
        "width": "100%",
        "height": "100%",
        "position": "absolute",
        "top": "0px",
        "left": "250px",
        "backgroundColor": "white",
        "border": "0px" 


    });

    var $iframe = $("<iframe>", {
        id: pageSrc,//without this, IE/Edge wont' work for cached iframe order
        src: "reports/" + pageSrc,
        'style': 'height:100%;width:100%; border:0px;  '
        //,'onload':"updateLayout(this)"
    });
    $div.append($iframe);
    $("#iframeDiv").append($div);

    return $div

}
//This function is not used anymore
function updateLayout(obj) {
    reportApp.trace = reportApp.trace + "<br><span>scrollWidth:" + obj.scrollWidth + " scrollHeight:" + obj.scrollHeight + "</span><br><span>windowWidth:" + $(window).width() + " windowHeight:" + $(window).height() + "</span>";

    var dataArea = obj;
    //var dataArea=obj.contentWindow.document.getElementById('data-area');

    reportApp.trace = reportApp.trace + "<span><br>Iframe:scrollWidth:" + dataArea.scrollWidth
	+ " clientWidth:" + dataArea.clientWidth
	//+" width:"+dataArea.width
	+ " offsetWidth:" + dataArea.offsetWidth
	+ "</span>";

    document.getElementById("Ytrace").innerHTML = reportApp.trace;
}

//hide a page
function hidePage(id) {


    var menuMap = reportApp.menuMap;
    var $this = this;
    //Need to get the current scrollX and scrollY first before layout is changed(hiding)  . 
    var current_scrollY = $(window).scrollTop();
    var current_scrollX = $(window).scrollLeft();

       menuMap.forEach(function (group) {

        group.pages.forEach(function (page) {

            if (page.id === id) {
                if (!!page.pageDivObj) {

                    page.pageDivObj.hide();
                    page.scrollY = current_scrollY;
                    page.scrollX = current_scrollX;


                }

            }


        });

    });


}


//This function is called by report.html when it receives message from iframe that its width/height is changed. 
//save width/height to page so we can revert it back among pages navigation
function UpdatePageWidthHeight(id, xy) {


    var menuMap = reportApp.menuMap;


    menuMap.forEach(function (group) {

        group.pages.forEach(function (page) {

            if (page.id === id) {
                page.width = xy.maxWidth;
                page.height = xy.maxHeight;

            }


        });

    });


}

function showPage(id) {
    if (id === reportApp.currentPageId) return;

    //hide first
    hidePage(reportApp.currentPageId);

    var menuMap = reportApp.menuMap;
    var $this = this;

    var target_scrollY = 0;
    var target_scrollX = 0;
    menuMap.forEach(function (group) {

        group.pages.forEach(function (page) {

            if (page.id === id) {
                if (!!page.pageDivObj) {
                    var ifrmDiv = document.getElementById('iframeDiv');

                    //revert back  the layout.
                    if (ifrmDiv) {
                        ifrmDiv.style.width = page.width + 80 + "px";
                        ifrmDiv.style.height = page.height + 80 + "px";
                    }


                    reportApp.currentPageId = id;
                    page.pageDivObj.show();
                    target_scrollY = page.scrollY;
                    target_scrollX = page.scrollX;


                }
                else //if the object is not created yet, then create it and set it visible
                {
                    //reset window size
                    var ifrmDiv = document.getElementById('iframeDiv');
                    if (ifrmDiv) {
                        ifrmDiv.style.width = 0 + "px";
                        ifrmDiv.style.height = 0 + "px";
                    }

                    page.pageDivObj = createDivIframe(id, page.src);
                    page.scrollY = 0;
                    page.scrollX = 0;
                    /*
                      $("#id").css("display", "none");
                      $("#id").css("display", "block");
                    */
                    page.pageDivObj.show();
                    reportApp.currentPageId = page.id;
                    target_scrollY = page.scrollY;
                    target_scrollX = page.scrollX;


                }
            }


        });

    });

    //Now it is time to scroll. need to hide page first. otherwise the layout is not correct.

    window.scrollTo(target_scrollX, target_scrollY);

    // reportApp.trace="<span>"+reportApp.trace+"</span><br><span>"+pageSrc+" target_scrollY:"+target_scrollY+"</span>";
    // document.getElementById("Ytrace").innerHTML=reportApp.trace;



}
 

function initMenuMap() {
    var menuMap = reportApp.menuMap;
    var i = 0;
    menuItems.forEach(function (page) {
        menuMap[getGroupIndex(page.group)].pages.push({
            id: i,
            src: page.src,
            pageName: page.menuTitle ? page.menuTitle : page.displayTitle,
            pageDivObj: null,
            width: 100, height: 100
        });
        i++;
    });

    function getGroupIndex(groupName) {
        for (var i = 0; i < menuMap.length; i++) {
            if (menuMap[i].groupName === groupName) {
                return i;
            }
        }
        menuMap.push({
            groupName: groupName,
            pages: []
        });
        return menuMap.length - 1;
    }
}

$.fn.drawMenu = function () {
    var menuMap = reportApp.menuMap;
    var $this = this;
    menuMap.forEach(function (group) {
        var $container = $('<div><h3><span>\u2212</span> {0}</h3></div>'.format(group.groupName.encodeHTML()));
        var $ul = $('<ul class="expanded"></ul>').appendTo($container);
        group.pages.forEach(function (page) {
            //  var $li = $('<li onMouseOver=\"this.style.color=\'RED\'\">{0}</li>'.format(page.pageName.encodeHTML()));
            var $li = $('<li>{0}</li>'.format(page.pageName.encodeHTML()));
            $li.click(function () {
                //reportApp.pages[reportApp.tableIndex[page.src]].show();
                showPage(page.id);
            });
            $ul.append($li);
        });
        $this.append($container);
    });
    this.on('click', 'h3', function () {
        var $h3 = $(this);
        var $ul = $h3.next();
        var $span = $h3.find('span');
        if ($ul.hasClass('expanded')) {
            $ul.attr('class', 'collapse');
            $span.html('+');
        } else {
            $ul.attr('class', 'expanded');
            $span.html('\u2212');
        }
    });
    $('#group-toggle').click(function () {
        var $a = $(this);
        var expanded = $a.hasClass('expanded');
        var $targetUL = $this.find('ul.{0}'.format(expanded ? 'expanded' : 'collapse'));
        $targetUL.prev().click();
        $a.attr('class', expanded ? 'collapse' : 'expanded').html(expanded ? '+ Expand All' : '\u2212 Collapse All');
    });
}