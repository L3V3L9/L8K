public: public_struct public_files

public_struct:
	@$(INFO) "restructuring public folder\n"
	@rm -Rf $(PUBLIC_PATH)
	@mkdir $(PUBLIC_PATH) ; mkdir $(PUBLIC_PATH)/js ; mkdir $(PUBLIC_PATH)/js/lib ; mkdir $(PUBLIC_PATH)/css ; mkdir $(PUBLIC_PATH)/img

public_files:
	@cp -R src/app/thewall/* public/
	@cp vendor/thewall/wall.js public/js/

rserve:
	@cp src/server/rserve/server.r /tmp/


#----------------------------------------------------------
# Paths
PUBLIC_PATH = public
VENDOR_PATH = vendor
LOG_PATH = log

#----------------------------------------------------------
# colors and color related macros
APP_NAME=L8K

NO_COLOR=\x1b[0m
INFO_COLOR=\x1b[36;01m
SUBJECT_COLOR=\x1b[33;01m
OK_COLOR=\x1b[32;01m
ERROR_COLOR=\x1b[31;01m
WARN_COLOR=\x1b[33;01m

OK_STRING=$(OK_COLOR)[OK]$(NO_COLOR)
ERROR_STRING=$(ERROR_COLOR)[ERRORS]$(NO_COLOR)
WARN_STRING=$(WARN_COLOR)[WARNINGS]$(NO_COLOR)

INFO = printf "$(INFO_COLOR)[$(APP_NAME)]$(NO_COLOR) " ; printf
ERR = printf "$(ERROR_COLOR)[$(APP_NAME)]$(NO_COLOR) " ; printf
#----------------------------------------------------------
