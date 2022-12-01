public interface LogListener
{
    void Header(string head);
    void Detail(string detail);
    void Clear();
}