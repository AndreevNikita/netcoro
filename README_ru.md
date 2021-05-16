![NetCoro logo](logo.png)

<br/>

### [Main readme (En)](README.md)

<br/>  

# NetCoro
NetCoro - концептуальная библиотека, добавляющая корутины в C#.

Преимущества корутин:
- один единственный основной поток
- возможность легко приостанавливать выполнение метода в ожидании какого-либо события
- отсутствие конкурентного доступа к ресурсам
- в некоторых случаях корутины могут упростить программы с паттерном "Цепочка обязанностей"

# Начало работы

## Корутина

В NetCoro изначально корутиной является объект, реализующий интерфейс **IEnumerable\<Awaitable\>**, который может последовательно возвращать ожидаемые объекты. Однако, такой итератор можно получить и из итератора **IEnumerable\<Coro\>**, последовательно получив все корутины и получив объекты **Awaitable** из них. Поэтому для удобства мы также будем называть корутинами объекты типа **IEnumerable\<Coro\>**. Такой итератор можно получить из методов с `yield return`, например:
```c#
public static IEnumerable<Coro> myCoro() {
	Console.WriteLine("Executable code before await");
	//...
	yield return Coro.Await(() => SomeLongMethod());
	//...
	Console.WriteLine("Executable code after await");
}
```

Для удобства в NetCoro абсолютно все исполняемые объекты приводятся к корутинам. Таким образом достигается возможность рекурсивного исполнения методов-итераторов **IEnumerable\<Coro\>**.

## Await

Для визуального удобства подготовка объекта к отдаче через `yield return` осуществляется через метод `Coro Coro.Await(...)`, перегруженный для разных возвращаемых объектов. Для каждой перегрузки статического метода `Coro Coro.Await(...)` существует метод расширения в статическом классе CoroExt (только для того, чтобы была возможность написать `myTask.Await()` вместо `Coro.Await(myTask)`).

В даннный момент предусмотрено ожидание объектов следующих типов:
- **Task**
- **Action** / **Func\<T\>** - из переданного в качестве аргумента делегата создаётся Task
- **WaitHandle**
- **DateTime** - приостановка исполнения корутины до определённого времени
- **int** / **double** / **TimeSpan** (приостановка исполнения корутины на определённое количество миллисекунд)

Также предусмотрены управляющие **Awaitable** объекты, которые можно получить через:
- `Coro.DoNothing();` - приостановить выполнение текущей корутины и перейти к следующей
- `Coro.InterruptCurrent();` - прервать выполнение текущей корутины
- `Coro.InterruptAll();` - прервать выполнение всех корутин и впоследствии выйти из метода **Work**


Итераторы **IEnumerable\<Coro\>** являются вложенными корутинами, при возврате корутины на основе **IEnumerable\<Coro\>** конечный ожидаемый объект **Awaitable** возвращается рекурсивно.

## AwaitLong

Ранее было сказано, что делегаты в конечном счёте приводятся и обрабатываются как **Task**. Метод **AwaitLong** используется для делегатов, которые планируется ожидать долго (например, ожидание ввода пользовтеля `yield return Coro.AwaitLong(Console.ReadLine);`). Фактически в **AwaitLong** задачи создаются с параметром `TaskCreationOptions.LongRunning`.


## Promise

После ожидания задачи или делегата результат выполнения можно получить по ссылке с помощью объекта **Promise** или **Promise\<T\>** в случае, если ожидаемый объект возвращает результат.

### Свойства и методы **Promise**:
- `TaskStatus Status` - статус завершённой задачи
- `Exception Exception` - исключение (если было выброшено) завершённой задачи
- `Asset()` - выбрасывает исключение, если задача не была успешно завершена из-за внутреннего исключения
- `AssertForward()` - выбрасывает исключение типа **ForwardException**, пробрасываемое из CoroManager в вызывающий метод в любом случае

Если задача возвращает результат, Await возвращает по ссылке объект **Promise\<TResult\>**, где **TResult** - тип возвращаемого результата. Данный класс является наследником **Pormise** и содержит все его свойства и методы, а так же:
- `TResult Result` - результат выполненной ожидаемой задачи

Пример:
```c#
public static IEnumerable<Coro> inputCoro() {

	yield return Coro.AwaitLong(() => Console.ReadLine(), out Promise<string> promise);
	promise.Assert();
	Console.WriteLine($"Task result: {promise.Result}");	
	
}
```

## CoroManager

**CoroManager** - объект, исполняющий корутины.

### Конструктор 
```c#
CoroManager(bool catchExceptions = true)
```
- `bool catchExceptions` - если true, при исполнении будут отлавливаться все исключения, кроме **ForwardException** и передаваться в обработку в **CoroExecutionController** корутины, из которой будет выброшено исключение

### Методы:
- `bool AddCoro(CoroExecutionController coroExecutorProperties)` - добавление корутины (контроллера выполнения) в **CoroManager**
- `void Work()` - метод, блокирующий текущий поток и выполняющий корутины, переданные с помощью метода **AddCoro**
- `void WaitAndStop()` - ожидание незавершённых корутин и остановка работы **CoroManager**. После остановки работы продолжается работа программы после вызова метода **Work**
- `void SkipWaitOnce()` - один раз пропускает ожидание объектов Awaitable, возвращённых корутинами
-  Метод расширения **AddCoro**
	```c#
	public static void AddCoro(
		this ICoroManager coroManager, 
		IEnumerable<Coro> coro, 
		Action<CoroExecutionController> finished = null,
		Action completed = null, 
		Action canceled = null, 
		Action<Exception> failtured = null,
		TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default
	) => coroManager.AddCoro(coro.Control(finished, completed, canceled, failtured, taskExceptionBehaviour));
	```
	Метод расширения для объектов **CoroManager**, позволяющий добавлять на исполнение итераторы путём создания внутри объекта **CoroExecutionController** и передачи в `bool AddCoro(CoroExecutionController coroExecutorProperties)`. Благодаря данному методу существует возможность добавить на исполнение не создавая **CoroExecutionController** явно `coroManager.AddCoro(myCoroMethod);`


## Статический CoroManager

Для случаев, когда нужно только дождаться исполнения нескольких корутин существует статический метод Work и его перегрузки
- `public static void Work(params IEnumerable<Coro>[] coros)`
- `public static void Work(params CoroExecutionController[] controlsArray)`
- `public static void Work(IEnumerable<CoroExecutionController> corosExectuionControllers)`
- `public static void Work(bool catchExceptions, params IEnumerable<Coro>[] coros)`
- `public static void Work(bool catchExceptions, params CoroExecutionController[] controlsArray)`
- `public static void Work(bool catchExceptions, IEnumerable<CoroExecutionController> corosExectuionControllers)`

Все методы принимают в качестве одного из аргументов исполняемые корутины или контроллеры исполнения. Второй возможный аргумент `bool catchExceptions` указывает, нужно ли отлавливать исключения внутри метода **Work** и штатно выполнять другие корутины или следует выбросить его наружу.

## CoroExecutionController

**CoroExecutionController** - объект для контроля исполнения одной корутины

### Конструктор:
`public CoroExecutionController(Coro coro, Action<CoroExecutionController> finished = null, Action completed = null, Action canceled = null, Action<Exception> failtured = null, TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default)`
- `Coro coro` - корутина, выполнение которой будет контролироваться
- `Action<CoroExecutionController> finished = null` - делегат, вызываемый по окончании выполнения корутины в любом случае. В качестве аргумента передаётся контроллер завершённой корутины
- `Action completed = null` - делегат, вызываемый при успешном окончании выполнения корутины
- `Action canceled = null` - делегат, вызываемый при отмене выполнения корутины (например, если в качестве ожидаемого объекта был возвращён `Coro.InterruptCurrent();` или `Coro.InterruptAll();`)
- `Action<Exception> failtured = null` - делегат, вызываемый в случае, если во время выполнения корутины было выброшено исключение (в случае, если **catchExceptions** в CoroManager было **true**)
- `TaskExceptionBehaviour taskExceptionBehaviour = TaskExceptionBehaviour.Default` - поведение при возникновении исключений в данной корутине по умолчанию (подробнее см. ниже)

### Свойства:
- `event Action<CoroExecutionController> Finished` - делегат, вызываемый по окончании выполнения корутины в любом случае. В качестве аргумента передаётся контроллер завершённой корутины
- `event Action Completed` - делегат, вызываемый при успешном окончании выполнения корутины
- `event Action Canceled` - делегат, вызываемый при отмене выполнения корутины (например, если в качестве ожидаемого объекта был возвращён `Coro.InterruptCurrent();` или `Coro.InterruptAll();`)
- `event Action<Exception> Failtured` - делегат, вызываемый в случае, если во время выполнения корутины было выброшено исключение (в случае, если **catchExceptions** в **CoroManager** было **true**)
- `Exception Exception` - исключение, выброшенное при исполнении корутины или **null**
- `bool IsFinished` - показывает, было ли завершено исполнение корутины

### Методы:
- `void Interrupt()` - прерывает исполнение корутины
- `void Wait()` - блокирует текущий поток в ожидании окончания выполнения корутины
- `InterruptAndWait()` - начинает прерывать исполнение корутины и блокирует текущий поток до остановки

Также в **CoroExt** имеются методы расширения **Control** с первым параметром **Coro** и **IEnumerable\<Coro\>**, по своей сути, он создан для удобства, все аргументы соответствуют аргументам конструктора **CoroExecutionController**


## Обработка исключений

Исключения в корутинах:
- Исключения, выбрасываемые из методов-итераторов отлавливаются и передаются в делегат Failtured контроллера исполенения
- Исключения типа **ForwardException** выбрасывается из метода Work всегда, подразумевается, что оно предназначено для обработки извне

Обработка исключений, возникших в **Awaitable**:
- При вызове метода **Await** для асинхронных задач (и делегатов) можно указать параметр **TaskExceptionBehaviour**
	- **TaskExceptionBehaviour.Nothing** - при возникновении исключения в Task ничего не происходит
	- **TaskExceptionBehaviour.ThrowInCoro** - при возникновении исключения в Task исключение выбрасывается в корутине в основном потоке
	- **TaskExceptionBehaviour.Default** - поведение определяется параметром **taskExceptionBehaviour**, переданным в конструктор контроллера исполнения текущей корутины (если при создании **CoroExecutionController** было так же передано значение **TaskExceptionBehaviour.Default**, не происходит ничего)
- Если возникло исключение, но не было выброшено автоматически с параметром **TaskExceptionBehaviour**, есть возможность проверить задачу на наличие исключения и выбросить его в случае, если оно возникло с помощью `promise.Asset()`
- Также можно вызвать исключение типа **ForwardException** через `promise.AssetForward()`

# Примеры

Все примеры можно найти в проекте [Examples](Examples/Examples)

## Пример 1 (Round Robin)

При запуске данного кода будет будут выполняться корутины, печатающие 30 раз символы "c", "a", "t", " ", передавая друг другу управление через `yield return Coro.DoNothing();`. В итоге в консоли выведется
> cat cat cat cat cat ...

```c#
public class Example1 {

	private static IEnumerable<Coro> printCoro(char c) {
		for(int index = 0; index < 30; index++) { 
			yield return Coro.DoNothing();
			Console.Write(c);
		}
	}

	public static void Start() { 
		CoroManager.Work(printCoro('c'), printCoro('a'), printCoro('t'), printCoro(' '));
	}

}
```

## Пример 2 (Long tasks and delays)


В данном примере **InputCoro** постоянно ожидает ввод пользователя, в это время могут выполняться корутины **Worker**, сортирующие строку, переданную в качестве аргумента **line**. При этом `yield return Task.Delay(...);` симулирует ожидание какой-то долгой операции, во время ожидания которой могут исполняться другие **Worker**'ы.

```c#
public class Example2 {
	private static CoroManager coroManager;
	private static int lineNumber;

	private static IEnumerable<Coro> InputCoro() { 
		while(true) { 
			yield return Coro.AwaitLong(() => Console.ReadLine(), out Promise<string> promise);
			coroManager.AddCoro(Worker(promise.Result, ++lineNumber));
		}
	}

	private static IEnumerable<Coro> Worker(string line, int lineNumber) { 
		//Difficult algorithm
		char[] arr = line.ToCharArray();
		Console.WriteLine($"Line {lineNumber}: {new string(arr)}");
		for(int index1 = 0; index1 < arr.Length; index1++) {
			for(int index2 = 0; index2 < arr.Length - 1; index2++) { 
				if(arr[index2] > arr[index2 + 1]) { 
					char buffer = arr[index2 + 1];
					arr[index2 + 1] = arr[index2];
					arr[index2] = buffer;
			
					//Some long operation
					yield return Task.Delay((int)new Random().Next(1000, 2000)).Await();
			
					Console.WriteLine($"Line {lineNumber}: {new string(arr)}");
				}
				
			}
		}
	}

	public static void Start() {
		coroManager = new CoroManager();
		coroManager.AddCoro(InputCoro());
		coroManager.Work();
	}
}
```

## Пример 3 (CoroExecutionController)

В данном примере продемострировано создание **CoroExectionController** для корутины и добавление callback-методов

```c#
public class Example3 {

	public enum CoroAction { SuccessCompletion, Interrupt, ThrowException }

	private static IEnumerable<Coro> TestCoro(CoroAction action) { 

		switch(action) { 
			case CoroAction.SuccessCompletion:
				yield break;
			case CoroAction.Interrupt:
				yield return Coro.InterruptCurrent();
				yield break;
			case CoroAction.ThrowException:
				throw new Exception("My exception");
		}
	}

	public static void Start() {
		//Create a CoroExecutionController
		foreach(CoroAction action in typeof(CoroAction).GetEnumValues()) {
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine($"{action.ToString()} run");
			Console.ForegroundColor = ConsoleColor.White;
			Console.WriteLine(new string('-', 80));

			//Create a CoroExecutionController for new TestCoro(action)
			CoroManager.Work(TestCoro(action).Control(
				finished: (c) => Console.WriteLine("TestCoro finished!"),
				completed: () => Console.WriteLine("TestCoro completed!"),
				canceled: () => Console.WriteLine("TestCoro canceled!"),
				failtured: (e) => Console.WriteLine($"Exception occured ({e.Message})!")
			));

			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
		}
	}

}
```

## Больше примеров

Больше примеров использования можно увидеть в проекте [Test](NetCoro/Test/Tests)

<br/><br/>

# Спасибо!
Спасибо Анастасии Даниловой ([@hakishima_art](https://www.instagram.com/hakishima_art)) за классный логотип проекта!