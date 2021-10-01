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
- [x] Serialize Print Templates
- [x] Serialize Export Templates

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
- [x] Restore Print Templates
- [x] Restore Export Templates

### Property Editors

- [x] Store Picker
- [x] Store Entity Picker
- [x] Price
- [x] Variants
