package main

import (
	"fmt"
	"os"
)

var outputFlag string = "scala.tests.12.8.sh"

func main() {
//	_threadList := []int{1,2,4,6, 8,10, 12}
	_intsList := []int{1000, 10000, 100000, 1000000, 10000000}

	iterations := 100



	//for _, _threads := range _threadList {
		for _, _ints := range _intsList {
			for j := 1; j<= 1; j++ {
				for i := 0; i < iterations; i++ {
					writeOutput(fmt.Sprintf("scala -J-Xmx3g Tests.Test -n %d \n", _ints), outputFlag)
				}
			}
		}
//	}

	fmt.Println("Done")
}

func writeOutput(out string, file string) {

	if _, err := os.Stat(file); os.IsNotExist(err) {
		os.Create(file)
	}

	f, err := os.OpenFile(file, os.O_APPEND|os.O_WRONLY, 0600)
	if err != nil {
		panic(err)
	}

	defer f.Close()

	if _, err = f.WriteString(out); err != nil {
		panic(err)
	}

}
