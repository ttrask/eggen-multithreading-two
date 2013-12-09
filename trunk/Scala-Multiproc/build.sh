if [ ! -d ./bin ] ; then 
	echo "bin directory doesn't exist"
	echo "creating bin directory"
   mkdir bin
fi


shopt -s nullglob
shopt -s dotglob

files=(bin/*)

if [ ${#files[@]} -gt 0 ];then
	echo "Clearing ./bin Directory"
	rm -r bin/*
fi

echo "Compiling Scala Applicatinos"
scalac -language:implicitConversions  -feature -deprecation  src/multicore/*.scala src/client/*.scala src/main/*scala src/math/*.scala src/optional/*.scala src/rpc/*.scala src/server/*.scala src/utility/*.scala src/stock/*.scala -d bin 

if [ -f ./clients.txt ]; then
	echo "Moving Clients.txt to ./bin/client/"
	cp ./clients.txt ./bin/client/clients.txt
fi

