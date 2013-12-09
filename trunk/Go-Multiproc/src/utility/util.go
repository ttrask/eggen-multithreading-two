package utility

/*
*TITLE: 	Go utility package
*AUTOHR: 	THOMAS TRASK
*STARTDATE: 12-5-2012
*
*PURPOSE: 	To help fascilitate generating and sorting stuff in GO
*
 */


import (
	"fmt"
	"math"
	"math/rand"
	"runtime"
	"sort"
	"time"
)

//REPLY - Used to store data & times when
//		  a response is sent back from the Sort method
type Reply struct {
	Data        []int
	SortTime    time.Duration
	CombineTime time.Duration
}

//Used to instantiate the util methods.
//because Go needs a struct to point to
//when doing RPC.
type Util struct {
}

var _threads int = 0
var _maxListSize int = 4

//prints the list given to it.
//not advised for large lists.
func PrintList(l []int, c chan int) {
	//	fmt.Println("Starting to print List")
	//	for _, element := range l {
	//		fmt.Println(element)
	//	}
	//
	c <- 0

	close(c)
}

//generates a list of random ints for teh list
//based on the size of the list given to it.
func GenerateList(list *[]int) {

	internal := *list

	for i, _ := range internal {

		internal[i] = rand.Intn(math.MaxInt32)
		//fmt.Println("List ", i, "=", internal[i])
	}

	*list = internal

}

//run an asyncronous sort sort on the list
func SortList(list *[]int, c chan int) {

	internal := *list
	sort.Ints(internal)
	*list = internal

	c <- 0

	close(c)

}

//Splits the given list of lists into smaller lists of lists
//and sorts them.
//
//During testing it was determined that, as the number of lists grows
//the performance of the normal CombineSortedLists method degraded
//significantly, proportinally by the equations  n = len(list)*len(m)
//where n is the number of iterations through  the recombination loop.
//A list with 64 blocks and 1 billion numbers
// would loop 64 billion times.
//CombineSortedListsThreaded shortens this down to the equation:
//n = #hops * len[m], where #hops is root(maxListSize, len(list))
//For a set of 64 lists containing 1billion #s and a maxlist size of
//4, n = root(4, 64)*1billion = 3billion, 4% the number of iterations
//as stated previously.  The downside is that memory usage for the set
//increases by the equation mem= size(m)*(#hops-1), as the memory is duplicated
//for each hop.
func CombineSortedListsThreaded(list [][]int, m []int, ch chan<- []int) {

	listCount := len(list)

	//	fmt.Println("ListSize = ", listCount)

	maxListSize := 4

	if listCount > maxListSize && listCount%maxListSize == 0 {

		c := listCount / maxListSize

		chans := make([]chan []int, maxListSize)

		ok := make([]bool, maxListSize)

		for i := 0; i < maxListSize; i++ {
			chans[i] = make(chan []int)
		}

		//		fmt.Println("Splitting list[", listCount,"] into ", maxListSize, " lists of size ", listCount/maxListSize)

		for i, val := range chans {

			l3 := list[i*c : (c * (i + 1))]
			//			defer close(chans[i])
			//			fmt.Printlnn(list)
			go CombineSortedListsThreaded(l3, m, val)
		}

		//	//loop until the channel is closed
		var allDone = false

		dat := make([][]int, len(chans))
		//		fmt.Println("dat len:", len(dat))

		for allDone == false {

			allDone = true

			for i, val := range chans {
				data, isDone := <-val
				//fmt.Println("dat[i]:", len(dat[i]))
				ok[i] = isDone

				if ok[i] == true {
					allDone = false
					dat[i] = make([]int, len(data))
					for k, val2 := range data {
						dat[i][k] = val2
					}
				} else {

				}
			}
		}

		d2 := CombineSortedLists(dat)
		//		fmt.Println("d2:", len(d2))
		ch <- d2

		close(ch)
	} else {

		ch <- CombineSortedLists(list)

		if ch != nil {
			close(ch)
		}
	}
}

//takes a set of sorted lists and combines them into one list
func CombineSortedLists(list [][]int) []int {

	listCount := len(list)

	//	fmt.Println("ListSize = ", listCount)

	//fmt.Println("Combining ", listCount, " slices into one.")
	listIndex := make([]int, listCount)
	i := 0
	lowman := math.MaxInt32
	val := 0
	lowmanIndex := -1

	setLength := 0
	listLengths := make([]int, len(list))

	for i, val := range list {

		listLengths[i] = len(val)
		setLength += listLengths[i]
	}

	j, k, l := 0, 0, 0
	m := make([]int, setLength)

	for i < setLength {

		for j = 0; j < listCount; j++ {

			k++
			if listIndex[j] < listLengths[j] {
				//				fmt.Println("Attempting to obtain list[", j, "][", listIndex[j], "]")
				l++
				val = list[j][listIndex[j]]

				//				fmt.Println("Comparing list[", j, "][", listIndex[j], "]:", val, " to lowman:", lowman, "; lowmanIndex=", lowmanIndex)
				if val <= lowman {
					//					fmt.Println("Repalcing lowman=", lowman, " with list[", j, "][", listIndex[j], "]=", val)
					lowman = val
					lowmanIndex = j
				}
			}
		}

		//		fmt.Println("found lowman[", i, "] on list[", lowmanIndex, "][", listIndex[lowmanIndex], "]=", lowman)

		//increments the "currently looked at value" for each list.		
		listIndex[lowmanIndex]++
		m[i] = lowman
		val = 0
		lowman = math.MaxInt32
		lowmanIndex = -1
		i++

	}

	//	fmt.Println("k=", k, "; l=", l)
	return m
}

//Sets the number of threads to use when splitting and sorting data.
//this directly correlates to the # of lists used in teh SortList &
//SortListThreaded methods.
func (u *Util) SetThreads(threads int, _return *string) error {

	if threads > 0 {
		_threads = threads
	}

	*_return = fmt.Sprintf("Set threads=%g", threads)

	return nil
}

//sorts a list and binds the sorted list, along with the
//time to sort and time to recombine, to the input Reply struct.
func (u *Util) Sort(m []int, m2 *Reply) error {

	fmt.Println("Recieved slice of size=", len(m), "for sorting.")

	//	for i := 0; i < 10; i++ {
	//		//fmt.Print(m[i], " ");
	//	}

	if _threads == 0 {
		_threads = runtime.NumCPU()
	}

	m2.Data = make([]int, len(m))

	runtime.GOMAXPROCS(_threads)

	list := make([][]int, _threads)
	ok := make([]bool, _threads)
	c := make([]chan int, _threads)

	remainder := len(m) % _threads

	start, end := 0, 0

	startTime := time.Now()

	for i := 0; i < _threads; i++ {

		//instantiates a channel/thread
		c[i] = make(chan int)

		//compensates for remainders when the
		//len(m)%(# of threads) != 0

		if remainder > 0 && i < remainder {
			start, end = (len(m)/(_threads))*i+i, (len(m)/(_threads))*(i+1)+i
		} else {
			start, end = (len(m)/(_threads))*i, (len(m)/(_threads))*(i+1)-1
			start, end = start+remainder, end+remainder
		}

		list[i] = make([]int, end-start+1)
		copy(list[i], m[start:end+1])
		//		fmt.Println("Sorting List", start, end)
		go SortList(&list[i], c[i])

	}

	var allDone = false

	for allDone == false {

		allDone = true
		for i := 0; i < _threads; i++ {

			_, ok[i] = <-c[i]
			if ok[i] == true {
				allDone = false
			}
		}
	}
	//wait until all lists are done.

	//Combine all lists into one list to return.
	//refactor this out to a global method
	//when you get a chance.

	m2.SortTime = time.Since(startTime)
	fmt.Println("Threaded Sort finished in ", m2.SortTime)
	fmt.Println("Starting Combination.")
	startTime = time.Now()

	//for performance reasons, it is advantageous to thread the recombination of
	//large datasets.  This is possible by a recursive call to the dataset
	//recomination routine that splits the dataset into more manageable pieces.
	if len(list) > _maxListSize && len(list)%_maxListSize == 0 {
		c := make(chan []int)
		go CombineSortedListsThreaded(list, m, c)

		//will return when M is done.

		m = <-c

	} else {

		m = CombineSortedLists(list)
	}

	m2.CombineTime = time.Since(startTime)
	fmt.Println("Combination done in ", m2.CombineTime)

	for i := 0; i < len(m); i++ {
		m2.Data[i] = m[i]
	}
	
	return nil
}


//removes the suffix from the input string s.
func TrimSuffix(s, suffix string) string {
	if HasSuffix(s, suffix) {
		return s[:len(s)-len(suffix)]
	}
	return s
}

//determiens if the input string s ends with the suffix string.
func HasSuffix(s, suffix string) bool {
	return len(s) >= len(suffix) && s[len(s)-len(suffix):] == suffix
}
