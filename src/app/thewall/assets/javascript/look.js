db = {

};
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
          if (items.length===0) return;
          var productIndex = detectProductIndices(items);
          if (productIndex[0]===undefined || productIndex[1]===undefined) return;
          var iid = mywall.getIdFromCoordinates(productIndex[1],productIndex[0]);
          //console.log($$("#"+iid));
          prods = [];
          if (db[iid]) {
             console.log(db[iid].pid); 
             console.log(db[iid].tags); 
             prods = [
                { id:db[iid].pid,tags:db[iid].tags }
             ];
          }
          request = {
             products : prods,
             items : items.length
          };
          var myRequest = new Request({ 
             url: '/discover',
             onSuccess: function(response) {
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
                                   object[i].imageUrl.replace(/.*?:\/\//g, "")+"&h=180&w=180)");
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
