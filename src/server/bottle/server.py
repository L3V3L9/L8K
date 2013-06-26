import sys, string, os
import json
from bottle import route, run, template
from bottle import static_file
from bottle import error,request
#import pyRserve
import operator
from platform_client import *
from rec_system import *
from datetime import datetime

#conn = pyRserve.connect()
#conn.eval("source('/tmp/server.r')")

cache = []
last_time = datetime.now()
@route('/static/<filepath:path>')
def server_static(filepath):
    return static_file(filepath, root='public/')

@error(404)
def error404(error):
    return 'Nothing here, sorry'

@route('/discover',method='POST')
def getdiscover():
    post_body = json.load(request.body)
    l = post_body['items']
    data = discover(l)
    return str(data)

def cacheandserve(topmost_tags,l):
    global cache
    n = datetime.now()
    tdelta = n - last_time
    last_time = n
    #print "DELTA IS " +  str(tdelta.total_seconds()) + " AND L is " + str(l)
    if tdelta.total_seconds()<2 and len(cache)>=l:
        #print "GET FROM CACHE"
        #serve from cache
        res = cache[:l]
        cache = cache[l:]
        return json.dumps(cache[:l])
    else:
        data = get_products_by_tags(topmost_tags)
        if (data):
            cache = json.loads(data)
            #print "CACHE SIZE IS  " + str(len(cache))
        else:
            data = "[]"
        return data
        
@route('/find',method='POST')
def getdiscover():
    print "---"
    print "#### next session ####"
    post_body = json.load(request.body)
    l = post_body['items']
    products = post_body['products']
    data = []
    if len(products):
        print "#### fetching pid #### =>" + str(products[0]['id'])
        topmost_tags_tuple = add_product_tagging_weight(products[0]['id'],5,products[0]['tags'])
        topmost_tags = []
        print "#### algo: input #### =>" + str(products[0]['id']) + " and tags: " + str(products[0]['tags'])
        print "#### algo: output #### =>" + str(topmost_tags_tuple)
        for t in topmost_tags_tuple:
            topmost_tags.append(t[0])
        next_tags = topmost_tags
        print "#### fetch tags (next-call) #### =>" + str(next_tags)
        data = cacheandserve(next_tags,l)
    return str(data)


@route('/random')
def get_rserve_random():
    l = request.query['items']
    lst = []
    if int(l)>0:
        items = str(conn.eval("item_choosing("+str(l)+")"))
        items = items[1:-1].translate(None,'\n').strip(' ')
        templst = [int(x) for x in items.split()]
        for j in templst:
            lst.append({'id':j,'tags':[1,2,3,4,5,6,7,8,9,10]})
    return str(lst)

print "## in case you want to push your own host\port just use the command line - when running the server."
print "## argv[1] = host, argv[2] = port"
print "#############"

host = '0.0.0.0'
if len(sys.argv) > 1:
	host = sys.argv[1]
	print 'argument #1: ' + sys.argv[1]

port = '3000'
if len(sys.argv) > 2:
	port = sys.argv[2]
	print 'argument #2: ' + sys.argv[2]

run(host=host, port=port)
