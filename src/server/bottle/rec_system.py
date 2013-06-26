import sys, string, os, struct, time
from collections import defaultdict
import operator

## 
# list<pid, weight, tag[]>

tags_storage = defaultdict(lambda: 0)
time_decay = 0.5

class Step(object):
	def __init__ (self, pid, direction_weight, ptags):
		self.pid = pid
		self.direction_weight = direction_weight
		self.ptags = ptags

class Suggestion(object):
	def __init__ (self, in_criteria, exit_criteria):
		self.next = in_criteria
		self.exit = exit_criteria
	def __str__(self):
		return "next: " + str(self.next) + "; exit: " + str(self.exit)

def reset_tag_storage():
	tags_storage = defaultdict(lambda: 0)

def add_product_tagging_weight_obj(step):
	return add_product_tagging_weight(step.pid, step.direction_weight, step.ptags)

def select_topmost():
	sorted_list_of_tags = sorted(tags_storage.iteritems(), key=operator.itemgetter(1))
	ret_value = sorted_list_of_tags[-6:]
	return ret_value

def select_bottom():
	sorted_list_of_tags = sorted(tags_storage.iteritems(), key=operator.itemgetter(1))
	ret_value = sorted_list_of_tags[:-6]
	return ret_value


def add_product_tagging_weight(pid, direction_weight, ptags):
	## fetch tags that relatedd to those pid
	run_decay_on_storage()
	for tag_id in ptags:
		tags_storage[tag_id] += direction_weight
	top_most = select_topmost()

	#next_tags = top_most + ptags
	#exit_tags = find_exit_criteria(next_tags)
	#res = Suggestion(next_tags, exit_tags)
	return top_most

def find_exit_criteria(in_criteria):
	tag_ids = [x for x in tags_storage if x not in in_criteria]
	return tag_ids


def run_decay_on_storage():
	for tag_id in tags_storage:
		value = tags_storage[tag_id]
		tags_storage[tag_id] = value * time_decay

# Recommendation System
if __name__ == "__main__":
	results = ""
	print add_product_tagging_weight(1, 3, [100, 400,200,300, 500])
	print add_product_tagging_weight(2, 5, [400, 500, 600])
	t1 = Step(pid=2, direction_weight=5, ptags=[900,800,700])
	print add_product_tagging_weight_obj(t1)
	print find_exit_criteria([100,200])