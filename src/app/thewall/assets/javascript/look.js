$(function() {    
    $('#wall .tile').on('mousedown',function() {
        switch(event.which) {
          case 1:
            var wall = $(this).closest('#wall');;
            wall.removeClass('not-clicked');
            wall.addClass('clicked');
            break;
          case 3:
            var tile = $(this);
            var pid = tile.data('pid');
            alert(pid);
            if (Platform != undefined && pid != undefined)
              Platform.Product.OpenQuickView(pid);
            break;
        }

        return false;
    })
    .on('mouseup', function(){
        var wall = $(this).closest('#wall');
        wall.removeClass('clicked');
        wall.addClass('not-clicked');
    });
});

db = {

};
originals = [];
init = false;
prods = [];
window.addEvent("domready", function(){

    var myRequest = new Request({ 
       url: "/reset",
       onSuccess: function(response) {
          console.log('server reset');
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
                db[iid].node.fade("hide").fade("in");
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

          var myRequest = new Request({ 
             url: endpoint,
             onSuccess: function(response) {
                var object = JSON.decode(response);
                items.each(function(e, i){
                   if (object[i]===undefined) return; 
                   var object_to_place = object[i];
                   if (init) {
                      var rand1= originals[Math.floor(Math.random() * originals.length)];
                      if ((e.x=== imin_x && e.y === imin_y) || (e.x===imax_x && e.y === imax_y)) {
                          object_to_place = originals[Math.floor(Math.random() * originals.length)];
                      }
                   }

                   var iid = mywall.getIdFromCoordinates(e.x,e.y);
                   db[iid] =  {
                      pid : object_to_place.id,
                      tags : object_to_place.tags,
                      node : e.node
                   };
                   if (init === false) {
                      originals.push(object_to_place);
                   }
                   if (init === false &&
                       e.x !== imin_x && e.x !== imax_x && e.y!==imax_y && e.y!==imin_y) {
                      console.log("setting to logo");
                      e.node.setStyle("backgroundImage", "url(assets/images/look.png)");
                   }
                   else {
                      $(e.node).attr('data-pid',object_to_place.id);
                      e.node.setStyle("backgroundImage",
                                      "url(http://images.weserv.nl/?url="+
                                      encodeURIComponent(object_to_place.imageUrl.replace(/.*?:\/\//g, ""))+"&h=180&w=180)");

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
