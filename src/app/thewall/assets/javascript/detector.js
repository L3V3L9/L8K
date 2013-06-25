amaxx = 0 ;amaxy=0;aminx = 0; aminy = 0;
function detectProductIndices(items) {
   var nmax_y = _.max(items,function(i) {
      return i.x;
   }).x;
   var nmax_x = _.max(items,function(i) {
      return i.y;
   }).y;
   var nmin_y = _.min(items,function(i) {
      return i.x;
   }).x;
   var nmin_x = _.min(items,function(i) {
      return i.y;
   }).y;

   var bx = nmin_x, 
   by = nmin_y,
   scanrows = true,scanline = true,
   lines = [0],
   ln = 0,
   last = -1,
   isHorizontalBar = false, 
   isVerticalBar = false;

   while (scanrows) {
      while (scanline) {
         if (_.findWhere(items,{x:by,y:bx })) {
            lines[ln] = lines[ln] + 1;
         }
         bx = bx + 1;
         if (bx > nmax_x) scanline = false;
      }
      if (last!==-1 && lines[ln]===0) {
         isVerticalBar = true;
         scanrows = false;
         break;
      }
      if (lines[ln]!==last && last!==-1) {
         console.log('break');
         break;
      }
      last = lines[ln];
      ln = ln + 1;
      lines[ln] = 0;
      by = by + 1;
      if (by > nmax_y) {
         scanrows = false;
         isHorizontalBar = true;
      }
      scanline = true;
   }
   var px,py;
   if (isHorizontalBar) {
      if (nmax_y > amaxy) {
         px = (nmax_x-Math.floor(((nmax_x-nmin_x)/2)));
         py = (nmax_y-1);
         console.log('horizontal bottom:'+px+','+py);
      }
      if (nmax_y < aminy) {
         px = (nmax_x-Math.floor(((nmax_x-nmin_x)/2)));
         py = (nmax_y+1);
         console.log('horizontal top:'+px+','+py);
      }
   }

   if (isVerticalBar) {
      if (nmax_x > amaxx) {
         py = (nmax_y-Math.floor(((nmax_y-nmin_y)/2)));
         px = (nmax_x-1);
         console.log('vertical right:'+px+','+py);
      }
      if (nmax_x < aminx) {
         py = (nmax_y-Math.floor(((nmax_y-nmin_y)/2)));
         px = (nmax_x+1);
         console.log('vertical left:'+px+','+py);
      }
   }
   amaxx = nmax_x;amaxy=nmax_y;aminx = nmin_x; aminy = nmin_y;
   if (px===undefined || py===undefined) {
      console.log('nmin_x='+nmin_x+',nmin_y='+nmin_y+',nmax_x='+nmax_x+',nmax_y='+nmax_y);
   }
   return [px,py];
}
