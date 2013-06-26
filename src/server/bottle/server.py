import sys, string, os
import json
from bottle import route, run, template
from bottle import static_file
from bottle import error,request
#import pyRserve
import operator
from platform_client import *
from rec_system import *

#conn = pyRserve.connect()
#conn.eval("source('/tmp/server.r')")

cache = []

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

@route('/find',method='POST')
def getdiscover():
    post_body = json.load(request.body)
    l = post_body['items']
    products = post_body['products']
    data = []
    if len(products):
        tags =  add_product_tagging_weight(products[0]['id'],5,products[0]['tags'])
        sorted_list_of_tags = sorted(tags.iteritems(), key=operator.itemgetter(1))
        print sorted_list_of_tags[-3:]
        topmost_tags = []
        for t in sorted_list_of_tags[-3:]:
            topmost_tags.append(t[0])
        data = get_products_by_tags(products[0]['tags'][0:3]+topmost_tags)
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
