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
                        var myRequest = new Request({ url: '/random',
                           onSuccess: function(response) {
                              var object = JSON.decode(response);
                              items.each(function(e, i){
                                 e.node.setStyle("backgroundImage","url(img/img"+object[i]+".jpg)");
                                 e.node.fade("hide").fade("in");
                              });
                        }}).get("items="+items.length);

                    }
                });
    // Init Wall
    mywall.initWall();
});
