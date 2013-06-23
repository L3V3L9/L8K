from bottle import route, run, template
from bottle import static_file
from bottle import error


@route('/static/<filepath:path>')
def server_static(filepath):
    return static_file(filepath, root='public/')

@error(404)
def error404(error):
    return 'Nothing here, sorry'


run(host='0.0.0.0', port=3000)
