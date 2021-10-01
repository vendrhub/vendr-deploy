# Vendr Deploy
Umbraco Deploy resolvers for Vendr, the eCommerce solution for Umbraco

Current primary focus is on syncing store settings, not orders, discounts or gift cards. Discounts and Gift Cards may come later, but right now it's just getting the store settings to work.

## Implemented

### Settings

#### Serializing

- [x] Serialize Stores
- [x] Serialize Order Statuses
- [x] Serialize Shipping Methods (Need to review ImageId)
- [x] Serialize Payment Methods (Need to review ImageId + Provider Settings)
- [x] Serialize Countries
- [x] Serialize Regions
- [x] Serialize Currencies
- [x] Serialize Tax Classes
- [x] Serialize Email Templates

#### Restoring

- [x] Restore Stores
- [x] Restore Order Statuses
- [x] Restore Shipping Methods
- [x] Restore Payment Methods
- [x] Restore Countries
- [x] Restore Regions
- [x] Restore Currencies
- [x] Restore Tax Classes
- [x] Restore Email Templates

### Property Editors

- [x] Store Picker
- [x] Store Entity Picker
- [x] Price
- [ ] Variants (future)
