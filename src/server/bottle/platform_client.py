import sys, string, os
import struct,time,hashlib,urllib

user_id=1114
app_id=20100
app_secret='a1a337326a5c40e99c413e4116315733'
base_url='http://shcboxplatform.shopyourway.com'

def discover():
	## Application ID 20100
	## Application Secret a1a337326a5c40e99c413e4116315733
	offlineToken=getOfflineToken(user_id,app_id)
	print "Offline Token: " + offlineToken
	hash = hashlib.sha256(offlineToken+app_secret).hexdigest()
	productIdsStrings = callEndpoint("/products/discover", offlineToken, hash, {'maxItems':100})
	commaSeperatedIds = productIdsStrings.strip('[').strip(']')
	productsDetails = callEndpoint("/products/get", offlineToken, hash, {'ids':commaSeperatedIds})
	return productsDetails




######  below you can find methods for internal usage ######



#################################################################################
# This method makes the call to the platform-api, 				  				#
#################################################################################
def callEndpoint(endpoint, token, hash, params):
	path = base_url + endpoint + "?"
	params['token'] = str(token)
	params['hash'] = str(hash)
	req_param=urllib.urlencode(params.items())
	print "## platform-call: " + path + req_param
	pageEndpoint=urllib.urlopen(path,req_param)
	results=pageEndpoint.read().replace('"','')
	return results



#################################################################################
# This method returns the signiture needed for getting the Offline Access Token #
#################################################################################
def getSignature(user_id,app_id,time_stamp,app_secret):
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
def getOfflineToken(user_id,appid_prod):
	uxtime=int(time.time())
	signature=getSignature(user_id,app_id,uxtime,app_secret)
	time_stamp_str=time.strftime("%Y-%m-%d %H:%M:%S",time.gmtime(uxtime)).replace(' ','T')
	req_url=base_url+'/auth/get-token?'
	req_param=urllib.urlencode([('userId',str(user_id)),('appId',str(appid_prod)),('timestamp',time_stamp_str),('signature',signature)])
	print "url:" + req_url
	print "params:" + req_param
	page=urllib.urlopen(req_url,req_param)
	token=page.read().replace('"','')
	return token