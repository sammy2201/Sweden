const COUNTY_IMAGE_PATHS: Readonly<Record<string, string>> = {
  blekinge: "/images/counties/Blekinge.jpeg",
  dalarna: "/images/counties/Dalarna.jpeg",
  gavleborg: "/images/counties/Gävleborg.jpeg",
  gotland: "/images/counties/gotland.jpeg",
  halland: "/images/counties/Halland.jpeg",
  jamtlandharjedalen: "/images/counties/Jämtland Härjedalen.jpg",
  jonkoping: "/images/counties/Jönköping.jpeg",
  kalmar: "/images/counties/Kalmar.jpeg",
  kronoberg: "/images/counties/Kronoberg.jpeg",
  norrbotten: "/images/counties/Norrbotten.jpeg",
  orebro: "/images/counties/Örebro.jpeg",
  ostergotland: "/images/counties/Östergötland.jpeg",
  skane: "/images/counties/skane.jpeg",
  sormland: "/images/counties/Sörmland.jpeg",
  stockholm: "/images/counties/stockholm.jpeg",
  uppsala: "/images/counties/Uppsala .jpeg",
  varmland: "/images/counties/Värmland.jpeg",
  vasterbotten: "/images/counties/Västerbotten.jpeg",
  vasternorrland: "/images/counties/Västernorrland.jpg",
  vastmanland: "/images/counties/Västmanland.jpeg",
  vastragotaland: "/images/counties/Västra Götaland.jpg",
};

const FALLBACK_COUNTY_IMAGE = "/images/stockholm-waterfront.jpg";

export function getCountyImagePath(countyId: string): string {
  return COUNTY_IMAGE_PATHS[countyId] ?? FALLBACK_COUNTY_IMAGE;
}
