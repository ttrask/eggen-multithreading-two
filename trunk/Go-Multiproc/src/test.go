package main 

import (
    "flag"
    "fmt" 
    "net/http"
)

type Celcius float32
type Farenheit float32
type Stringer interface{
	String() string
}
func (c Celcius) String() string{

	f := Farenheit(c*9/5+32)
	return fmt.Sprint(" %.2f°F", f)
}

type Vertex struct {
	x, y float64	
}

func (v Vertex) String() string{
	return fmt.Sprintf("%.2f, %.2f",v.x, v.y)
	
}


const APP_VERSION = "0.1"

// The flag package provides a default help printer via -h switch
var versionFlag *bool = flag.Bool("v", false, "Print the version number.")

func main() {

	http.HandleFunc("/", handler)
	http.ListenAndServe(":8080", nil)

    flag.Parse() // Scan the arguments list 

	fmt.Print("Version:", APP_VERSION)
    if *versionFlag {
        
    }
}

func handler(w http.ResponseWriter, r *http.Request){

	name := r.URL.Path[1:]
	fmt.Fprint(w, "hello, " + name)
	var v Vertex
	v.x = 5.0
	v.y = 10.0
	c := Celcius(0);
	fmt.Println(c);
	fmt.Fprint(w, v)
	fmt.Fprint(w, c)
}
