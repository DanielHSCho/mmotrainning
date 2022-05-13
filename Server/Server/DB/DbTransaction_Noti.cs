using Microsoft.EntityFrameworkCore;
using Server.Game;

namespace Server.DB
{
    public partial class DbTransaction : JobSerializer
    {
        public static void EquipItemNoti(Player player, Item item)
        {
            if (player == null || item == null) {
                return;
            }

            ItemDb itemDb = new ItemDb() {
               ItemDbId = item.ItemDbId,
               Equipped = item.Equipped
            };

            Instance.Push(() => {
                using (AppDbContext db = new AppDbContext()) {
                    db.Entry(itemDb).State = EntityState.Unchanged;
                    db.Entry(itemDb).Property(nameof(itemDb.Equipped)).IsModified = true;

                    bool success = db.SaveChangesEx();
                    if (success == false) {
                        // 실패했다면 Kick
                    }
                }
            });
        }
    }
}
