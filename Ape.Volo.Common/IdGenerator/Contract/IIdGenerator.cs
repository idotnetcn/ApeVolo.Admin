namespace Ape.Volo.Common.IdGenerator.Contract;

public interface IIdGenerator
{
    /// <summary>
    /// 生成一个新的Id
    /// </summary>
    /// <returns></returns>
    long NewId();
}
