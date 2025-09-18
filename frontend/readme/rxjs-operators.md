rxjs or reactive or stream is this weird almost new programming paradigm that i have ever encounter 
while i have worked with live data in android before, this time is the first time 
i really get my hand dirty with reactive programming 

(some topics here...)


Rxjs operator 
now imagine a reactive variable, Observable, Subject, BehaviorSubject, etc 
is just a pipe that transfer water, and inside the water, occasionally will have food 
the water transport food via the pipe 
the food is the data that we want, e.g., data from an api, that you want to send 
to your app
now this pipe, we can do whatever we want with it, with rxjs operators
the most common rxjs operators 
- pipe 
- subscribe 

what does pipe do
this.someObs<T>().pipe()
- this basically we gain access to the pipe that ive talked about earlier 
- once we have access to the pipe, we can do many things, hence bunch on rxjs operator that we can use inside pipe
    - tap() --> look at the data and do something that doesnt cause side effect (changing the data). most common use case is, logging, use the data to assign to local variables, etc...
    - map() --> an opposite of tap, this will change the data and the obs will return the changed/modified data 
    - filter() --> this doesnt change the data, just filter, e.g., api sends number 1-10, and we filter only even number, so the obs will only return 2, 4, 6, 10
    - switchMap() --> <fill up what this does>
    - 
