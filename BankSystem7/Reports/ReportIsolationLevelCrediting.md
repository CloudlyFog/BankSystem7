## Report of performance
### Benchmarks
![Benchmark](https://i.imgur.com/idp2dxS.png)

<hr>

When we use isolation level serializable we have growth of performance<br>
I'm confused by this situation.

**Annotation**<br>
The tests went through 30 iterations.<br>
Example:
```
for (int i = 0; i < 30; i++)
{
    bankContext.IsolationLevel = IsolationLevel.Serializable;
    bankContext.TakeCredit(bankAccount, credit);
}
```
and

```
for (int i = 0; i < 30; i++)
{
    bankContext.IsolationLevel = IsolationLevel.RepeatableRead;
    bankContext.TakeCredit(bankAccount, credit);
}
``````