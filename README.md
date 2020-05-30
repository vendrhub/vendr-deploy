# vendr-deploy
Umbraco Deploy resolvers for Vendr, the eCommerce solution for Umbraco v8+

Current primary focus is on syncing store settings, not orders, discounts or gift cards. Discounts and Gift Cards may come later, but right now it's just getting the store settings to work.

## TODO

### Settings

#### Serializing

- [x] Serialize Stores
- [x] Serialize Order Statuses
- [ ] Serialize Shipping Methods
- [ ] Serialize Payment Methods
- [ ] Serialize Countries
- [ ] Serialize Regions
- [ ] Serialize Currencies
- [x] Serialize Tax Classes
- [x] Serialize Email Templates

#### Restoring

- [ ] Restore Stores
- [ ] Restore Order Statues
- [ ] Restore Shipping Methods
- [ ] Restore Payment Methods
- [ ] Restore Countries
- [ ] Restore Regions
- [ ] Restore Currencies
- [ ] Restore Tax Classes
- [ ] Restore Email Templates

### Property Editors

- [x] Store Picker
- [x] Store Entity Picker
- [x] Price
- [ ] Variants (future)