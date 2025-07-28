namespace Domain.Constants;

public class KeycloakUser
{
    public Guid ID { get; set; }
    public bool Enabled { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public Dictionary<string, string[]> Attributes { get; set; }
    public List<string> RealmRoles { get; set; } = new();
}
/* EXAMPLE OUTPUT OF JSON DESERIALIZATION
 * {"id":"2af5dc02-faea-42f3-9245-44384d965b92","username":"testuser","email":"testuser@onet.pl",
 * "emailVerified":false,"attributes":{"profileCompleted":["true"], "avatarPath":[/uploads/avatars/default.png"]},"enabled":true,
 * "createdTimestamp":1753274517777,"totp":false,"disableableCredentialTypes":[],"requiredActions":[],"notBefore":0,
 * "access":{"manageGroupMembership":true,"view":true,
 * "mapRoles":true,"impersonate":false,"manage":true}}
 */