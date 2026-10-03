using Ukinee.Infrastructure.Ddd.Common.UnitOfWork.Contacts;

namespace Ukinee.Infrastructure.Ddd.Tests.Common.Utils.Mocks;

public class UnitOfWorkFactoryMock : IUnitOfWorkProvider, IUnitOfWorkFactory
{
    public IEditableUnitOfWork? Current { get; }

    public IUnitOfWork Create<TTag>()
    {
        throw new NotImplementedException();
    }
}
