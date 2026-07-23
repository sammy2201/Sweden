export enum Category {
  Architecture = "Architecture",
  Beach = "Beach",
  Castle = "Castle",
  Family = "Family",
  Food = "Food",
  Hiking = "Hiking",
  Historic = "Historic",
  Museum = "Museum",
  Nature = "Nature",
  Shopping = "Shopping",
}

export interface County {
  id: string;
  name: string;
  description: string;
  bannerImageUrl: string;
  featuredAttractionCount: number;
  defaultOrigin: string;
}

export interface Attraction {
  id: string;
  countyId: string;
  name: string;
  category: Category;
  shortDescription: string;
  description: string;
  estimatedVisitDuration: string;
  rating: number;
  city: string;
  tags: string[];
  imageUrl: string;
  openingHours: string;
  entryFee: string;
  address: string;
  officialWebsite: string;
  googleMapsUrl: string;
  nearestStation: string;
}
