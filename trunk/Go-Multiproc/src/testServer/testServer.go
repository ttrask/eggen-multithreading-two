package main

import (
//	"errors"
	"log"
	"net"
	"net/http"
	"utility"
	"net/rpc"
)



func main() {
	arith := new(utility.Arith)
	rpc.Register(arith)
	rpc.HandleHTTP()
	l, e := net.Listen("tcp", ":1234")
	if e != nil {
		log.Fatal("listen error:", e)
	}
	http.Serve(l, nil)
}
