using SAPbobsCOM;

namespace SapMiddleware.Interfaces;

public interface ISapConnectionManager
{
    Company ConnectCompany();
    void DisconnectCompany(Company? company);
}
