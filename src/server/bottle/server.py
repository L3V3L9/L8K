from bottle import route, run, template
from bottle import static_file
from bottle import error

@route('/')
def index():
	return static_file('index.html', root='public/')

@route('/help')
def help():
	return static_file('index.html', root='public/help/')


@route('/static/<filepath:path>')
def server_static(filepath):
    return static_file(filepath, root='public/static/')


@route('/hello/')
@route('/hello/<name>')
def Hello(name='World'):
    return template('<b>Hello {{name}}</b>!', name=name)


@error(404)
def error404(error):
    return 'Nothing here, sorry'


run(host='localhost', port=3000)