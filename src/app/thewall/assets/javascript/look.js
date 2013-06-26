$(function() {    
    $('#wall').mousedown(function(e){
        var wall = $(this);
        wall.removeClass('not-clicked');
        wall.addClass('clicked');
        e.preventDefault();
    })
    .mouseup(function(){
        var wall = $(this);
        wall.removeClass('clicked');                
        wall.addClass('not-clicked');
    });

    $('.tile').bind("contextmenu",function() {
      var tile = $(this);
      var pid = tile.data('pid');

      Platform.Product.OpenQuickView(pid);

      return true;
    });
});

db = {

};
init = false;
prods = [];
window.addEvent("domready", function(){

    var myRequest = new Request({ 
       url: "/reset",
       onSuccess: function(response) {
          alert('go');
       }
    }).get();

    colors = ["#730046", "#BFBB11", "#FFC200", "#E88801", "#C93C00"];
    var mywall = new Wall("wall", {
       "draggable":true,
       "inertia":true,
       "width":180,
       "height":180,
  //     "printCoordinates":true,
       "rangex":[-300,300],
       "rangey":[-300,300],
       callOnUpdate: function(items){
          var iid = -999;
          var productIndex,cx,cy;
          if (items.length===0) return;
          if (init) {
             productIndex = detectProductIndices(items);
             if (productIndex[0]===undefined || productIndex[1]===undefined) return;
             cx = productIndex[1],cy=productIndex[0];
             //console.log('ocx='+cy+',ocy='+cx);
             var safety = 0;
             //console.log(db);
             iid = mywall.getIdFromCoordinates(cx,cy);
             var incx = productIndex[3],
                 incy = productIndex[2];
             while (db[iid]===undefined && safety < 5) {
                //console.log(iid);
                cx = cx + incx;
                cy = cy + incy;
                iid = mywall.getIdFromCoordinates(cx,cy);
                //console.log('cx='+cy+',cy='+cx);
                safety = safety + 1;
             }
             //console.log($$("#"+iid));

             if (db[iid]) {
                // get 2 more products
                var iid1,iid2;
                if (incx === 0) {
                   iid1 = mywall.getIdFromCoordinates(cx-1,cy);
                   iid2 = mywall.getIdFromCoordinates(cx+1,cy);
                }
                if (incy === 0) {
                   iid1 = mywall.getIdFromCoordinates(cx,cy-1);
                   iid2 = mywall.getIdFromCoordinates(cx,cy+1);
                }
                console.log('cx='+cy+',cy='+cx);
                console.log(db[iid].pid); 
                console.log(db[iid].tags); 
                prods = [
                   { id:db[iid].pid,tags:db[iid].tags }
                ];
                if (db[iid1]) prods.push({ id:db[iid1].pid,tags:db[iid1].tags });
                if (db[iid2]) prods.push({ id:db[iid2].pid,tags:db[iid2].tags });
             }

          }
          request = {
             products : prods,
             items : items.length
          };
          var endpoint = '/discover';
          if (init) endpoint = '/find';
          else {
             var imax_x = _.max(items,function(i) {
                return i.x;
             }).x;
             var imax_y = _.max(items,function(i) {
                return i.y;
             }).y;
             var imin_x = _.min(items,function(i) {
                return i.x;
             }).x;
             var imin_y = _.min(items,function(i) {
                return i.y;
             }).y;
          }

          var myRequest = new Request({ 
             url: endpoint,
             onSuccess: function(response) {
                var object = JSON.decode(response);
                items.each(function(e, i){
                   if (object[i]===undefined) return; 
                   var iid = mywall.getIdFromCoordinates(e.x,e.y);
                   db[iid] =  {
                      pid : object[i].id,
                      tags : object[i].tags
                   };
                   if (init === false &&
                       e.x !== imin_x && e.x !== imax_x && e.y!==imax_y && e.y!==imin_y) {
                      console.log("setting to logo");
                      e.node.setStyle("backgroundImage", "url(assets/images/look.png)");
                   }
                   else {
                      $(e.node).attr('data-pid',object[i].id);
                      e.node.setStyle("backgroundImage",
                                      "url(http://images.weserv.nl/?url="+
                                      encodeURIComponent(object[i].imageUrl.replace(/.*?:\/\//g, ""))+"&h=180&w=180)");

                   }
                   e.node.fade("hide").fade("in");
                });
                init = true;
             }
          }).post(JSON.stringify(request));
       }
    });
    // Init Wall
    mywall.initWall();
});
