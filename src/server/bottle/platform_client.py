import sys, string, os
import struct,time,hashlib,urllib2,urllib

user_id=1114
app_id=20100
app_secret='a1a337326a5c40e99c413e4116315733'
base_url='http://shcboxplatform.shopyourway.com'
debug_mode=False


#class Memoize:
#	def __init__(self, func):
#		self.f = func
#		self.memo = {}
#	def __call__(self, *args):
#		str_args = str(args)
#		if not str_args in self.memo:
#			self.memo[str_args] = self.f(*args)
#		return self.memo[str_args]


def predefined_items(num):
	predefined_ids=[222022648,254720849,238970266,275362706]  #girls dresses => 1077004
	predefined_ids=predefined_ids+[165347484,199683497,202781100,258132401] #Junior Dresses => 1077042
	predefined_ids=predefined_ids+[168235051,642573,159771132,196581618,207306395,576048,1237085,928787,553417,1237094] #all_books => 1076814
	predefined_ids=predefined_ids+[96901779,61397513,115575988,96901779,181795083,175273737,218946090,107113903,260262586,185306236,57740888,104724145,196695167,254786237,235690645,197790954,225839494,92252296,316932,23063323] #Gadgets => 820741
	predefined_ids=predefined_ids+[254618075,208831469,166598331,251839945] #headphones => 4129732
	predefined_ids=predefined_ids+[244831502,223108437,155960188,279313775,178399727,182826022,262453453] #skateboards => 1077796
	predefined_ids=predefined_ids+[77589736,171801177,203688961,40292570] #Telescopes => 1077796
	predefined_ids=predefined_ids+[59616645,149798451,140996712,140990168,231831141,140738654,148012978] #Ty Penington + patio
	predefined_ids=predefined_ids+[65363912,109415633,144989276,65363908,159490575,22211085,336474,37287519,86820281] # New Born => 1460472
	predefined_ids=predefined_ids+[153316557,170568681,179358871,132559923,140132836,147324242,162752159,204552481] #Black and Yellow => 1143338
	predefined_ids=predefined_ids+[58469718,25787,249902976,192557909,277764161,255401760,277764133,277764131,192557960,277764159,277205734] # Cylcing => 899627
	#predefined_ids=predefined_ids+[] #womens-shoes/6875469
	predefined_ids=predefined_ids+[186795917,223437721,220189935,91541880,124263162,195925688] #girls-shoes/6876025/
	predefined_ids=predefined_ids+[294250,294352,294355,182343068,129676028,269354] #mens-work-shoes-boots/6875471/
	predefined_ids=predefined_ids+[161969674,186546079,304257,186546211,281805] #womens-dress-shoes/7856424/
	comma_seperated_ids = str(predefined_ids)
	comma_seperated_ids = comma_seperated_ids[1:-1]
	offline_token=get_offline_token(user_id,app_id)
	debug_it("Offline Token: " + offline_token)
	hash = hashlib.sha256(offline_token+app_secret).hexdigest()
	products_details = call_endpoint("/products/get", offline_token, hash, {'ids':comma_seperated_ids, 'with':'tags'})
	return products_details


def discover(limit=100):
	offline_token=get_offline_token(user_id,app_id)
	debug_it("Offline Token: " + offline_token)
	hash = hashlib.sha256(offline_token+app_secret).hexdigest()
	product_ids_strings = call_endpoint("/products/discover", offline_token, hash, {'maxItems':limit})
	comma_seperated_ids = product_ids_strings[1:-1]
	products_details = call_endpoint("/products/get", offline_token, hash, {'ids':comma_seperated_ids, 'with':'tags'})
	return products_details


def get_products_by_tags(tag_ids):
	offline_token=get_offline_token(user_id,app_id)
	debug_it("Offline Token: " + offline_token)
	hash = hashlib.sha256(offline_token+app_secret).hexdigest()
	new_tag_ids = remove_black_listed_tags(tag_ids)
	comma_seperated_tag_ids = str(new_tag_ids)[1:-1]
	products_details = call_endpoint("/products/get-by-tags", offline_token, hash, {'tagIds':comma_seperated_tag_ids, 'with':'tags'})
	return products_details

def remove_black_listed_tags(tag_ids):
	tags_to_exclude = [414208,479499,479516,5601110,5031571,3923331,4125531,6374908,5066939,474896,1776576,1962220,5969363,5031419,4785512,1098204,1098227,1112005,1173137,1245774,1324787,1324813,1611877,760011,760034,760038,760024,760057,760012,760013,760059,760063,760021,760067,760054,760001,760074,760027,760007,760053,760055,760010,760051,760006,760071,760018,760015,760069,760019,760023,760066,760008,760016,760040,760005,760050,760056,760070,760052,760047,760002,760041,760035,760022,760028,760033,760060,760075,760044,760046,760032,760065,760061,760003,760064,760025,760072,760058,760062,760037,760030,760045,760029,760020,760026,760073,760043,760031,760009,760068,760036,760049,760039,760017,760048,760004,760042,224510,221554,479499]
	new_tags_list = [x for x in tag_ids if x not in tags_to_exclude]
	return new_tags_list

######  below you can find methods for internal usage ######



#################################################################################
# This method makes the call to the platform-api, 				  				#
#################################################################################
def call_endpoint(endpoint, token, hash, params):
	path = base_url + endpoint + "?"
	params['token'] = str(token)
	params['hash'] = str(hash)
	req_param=urllib.urlencode(params.items())
	debug_it("## platform-call: " + path + req_param)
	pageEndpoint=urllib2.urlopen(path,req_param,timeout=5)
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
	debug_it("url:" + req_url)
	debug_it("params:" + req_param)
	page=urllib2.urlopen(req_url,req_param,timeout=5)
	token=page.read().replace('"','')
	return token

def debug_it(text):
	if debug_mode==True: 
		print text



## Scripting \ Main
if __name__ == "__main__":
	import sys
	debug_mode=True
	print predefined_items(1)
	#print discover()
	#print remove_black_listed_tags([414208, 414209])
	print get_products_by_tags([414208, 414209])
	exit(0)

