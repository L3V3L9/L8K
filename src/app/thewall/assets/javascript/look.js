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
});

db = {

};
init = false;
prods = [];
window.addEvent("domready", function(){
    colors = ["#730046", "#BFBB11", "#FFC200", "#E88801", "#C93C00"];
    var mywall = new Wall("wall", {
       "draggable":true,
       "inertia":true,
       "width":180,
       "height":180,
       "printCoordinates":true,
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
             while (db[iid]===undefined && safety < 5) {
                //console.log(iid);
                cx = cx + productIndex[3];
                cy = cy + productIndex[2];
                iid = mywall.getIdFromCoordinates(cx,cy);
                //console.log('cx='+cy+',cy='+cx);
                safety = safety + 1;
             }
             //console.log($$("#"+iid));

             if (db[iid]) {
                console.log('cx='+cy+',cy='+cx);
                console.log(db[iid].pid); 
                console.log(db[iid].tags); 
                prods = [
                   { id:db[iid].pid,tags:db[iid].tags }
                ];
             }

          }
          request = {
             products : prods,
             items : items.length
          };
          var endpoint = '/discover';
          if (init) endpoint = '/find';
          var myRequest = new Request({ 
             url: endpoint,
             onSuccess: function(response) {
                init = true;
                var object = JSON.decode(response);
                items.each(function(e, i){
                   if (object[i]===undefined) return; 
                   var iid = mywall.getIdFromCoordinates(e.x,e.y);
                   db[iid] =  {
                      pid : object[i].id,
                      tags : object[i].tags
                   };
                   e.node.setStyle("backgroundImage",
                                   "url(http://images.weserv.nl/?url="+
                                   encodeURIComponent(object[i].imageUrl.replace(/.*?:\/\//g, ""))+"&h=180&w=180)");
                   //e.node.setStyle("backgroundImage","url("+object[i].imageUrl+")");
                   //e.node.setStyle("backgroundSize","180px 180px");
                   e.node.fade("hide").fade("in");
                });
             }
          ///}).get("items="+items.length);
          }).post(JSON.stringify(request));
       }
    });
    // Init Wall
    mywall.initWall();
});
