# item choosing function
item_choosing = function(items_number)
{
  items = as.integer(runif(as.numeric(items_number),1,99));
  return(items);
}
