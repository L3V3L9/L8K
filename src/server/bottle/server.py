from bottle import route, run, template
from bottle import static_file
from bottle import error

@route('/')
def index():
	return template('Hola')


@route('/hello/')
@route('/hello/<name>')
def Hello(name='World'):
    return template('<b>Hello {{name}}</b>!', name=name)


@route('/static/<filepath:path>')
def server_static(filepath):
    return static_file(filepath, root='./static/')

@route('/views/<filepath:path>')
def server_views(filepath):
    return static_file(filepath, root='./views/')


@route('/help')
def help():
	return static_file('index.html', root='views/help/')


@error(404)
def error404(error):
    return 'Nothing here, sorry'


run(host='localhost', port=8080)