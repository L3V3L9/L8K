import sys, string, os, struct, time
from collections import defaultdict

## 
# list<pid, weight, tag[]>

tags_storage = defaultdict(lambda: 0)
time_decay = 0.5

class Step(object):
	def __init__ (self, pid, direction_weight, ptags):
		self.pid = pid
		self.direction_weight = direction_weight
		self.ptags = ptags


def add_product_tagging_weight_obj(step):
	return add_product_tagging_weight(step.pid, step.direction_weight, step.ptags)

def add_product_tagging_weight(pid, direction_weight, ptags):
	## fetch tags that relatedd to those pid
	run_decay_on_storage()
	for tag_id in ptags:
		tags_storage[tag_id] += direction_weight
	return tags_storage

def run_decay_on_storage():
	for tag_id in tags_storage:
		value = tags_storage[tag_id]
		tags_storage[tag_id] = value + (value * time_decay)

# Recommendation System
if __name__ == "__main__":
	results = ""
	print add_product_tagging_weight(1, 3, [100, 400,200,300, 500])
	print add_product_tagging_weight(2, 5, [400, 500, 600])
	t1 = Step(pid=2, direction_weight=5, ptags=[900,800,700])
	print add_product_tagging_weight_obj(t1)