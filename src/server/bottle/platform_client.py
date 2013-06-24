import sys, string, os
import struct,time,hashlib,urllib2,urllib

user_id=1114
app_id=20100
app_secret='a1a337326a5c40e99c413e4116315733'
base_url='http://shcboxplatform.shopyourway.com'

def discover(limit=100):
	offline_token=get_offline_token(user_id,app_id)
	print "Offline Token: " + offline_token
	hash = hashlib.sha256(offline_token+app_secret).hexdigest()
	product_ids_strings = call_endpoint("/products/discover", offline_token, hash, {'maxItems':limit})
	comma_seperated_ids = product_ids_strings[1:-1]
	products_details = call_endpoint("/products/get", offline_token, hash, {'ids':comma_seperated_ids, 'with':'tags'})
	return products_details


def get_products_by_tags(tag_ids):
	offline_token=get_offline_token(user_id,app_id)
	print "Offline Token: " + offline_token
	hash = hashlib.sha256(offline_token+app_secret).hexdigest()
	products_details = call_endpoint("/products/get-by-tags", offline_token, hash, {'tagIds':tag_ids})
	return products_details


######  below you can find methods for internal usage ######



#################################################################################
# This method makes the call to the platform-api, 				  				#
#################################################################################
def call_endpoint(endpoint, token, hash, params):
	path = base_url + endpoint + "?"
	params['token'] = str(token)
	params['hash'] = str(hash)
	req_param=urllib.urlencode(params.items())
	print "## platform-call: " + path + req_param
	pageEndpoint=urllib2.urlopen(path,req_param,timeout=2)
	results=pageEndpoint.read()
	return results



#################################################################################
# This method returns the signiture needed for getting the Offline Access Token #
#################################################################################
def get_signature(user_id,app_id,time_stamp,app_secret):
	userid_buffer=buffer(struct.pack('@q', user_id),0)
	appid_buffer=buffer(struct.pack('@q', app_id),0)
	timestamp_buffer=buffer(struct.pack('@d',time_stamp),0)
	appsecret_buffer=buffer(bytearray(ord(app_secret[i]) for i in range(0,len(app_secret))),0)
	new_hash=hashlib.sha256()
	new_hash.update(str(userid_buffer)+str(appid_buffer)+str(timestamp_buffer)+str(appsecret_buffer))
	signature=new_hash.hexdigest()
	return signature

#################################################################################
# This method returns the token needed for creating the hash                    #
#################################################################################
def get_offline_token(user_id,appid_prod):
	uxtime=int(time.time())
	signature=get_signature(user_id,app_id,uxtime,app_secret)
	time_stamp_str=time.strftime("%Y-%m-%d %H:%M:%S",time.gmtime(uxtime)).replace(' ','T')
	req_url=base_url+'/auth/get-token?'
	req_param=urllib.urlencode([('userId',str(user_id)),('appId',str(appid_prod)),('timestamp',time_stamp_str),('signature',signature)])
	print "url:" + req_url
	print "params:" + req_param
	page=urllib2.urlopen(req_url,req_param,timeout=2)
	token=page.read().replace('"','')
	return token



## Scripting \ Main
if __name__ == "__main__":
	import sys
	results = ""
	print "syntax: python platfrom_client.py [method_name] [method_variable]"
	if len(sys.argv) < 2:
		exit(1)

	if len(sys.argv) >= 2:
		method_name = str(sys.argv[1])
		if method_name == "discover":
			if len(sys.argv) == 3:
				results = discover(int(sys.argv[2]))
			else:
				result = discover()
		elif method_name == "get_products_by_tags": 
			results = get_products_by_tags("220420,414208")
		else:
			print "can't run method " + method_name
			exit(1)

	print results
