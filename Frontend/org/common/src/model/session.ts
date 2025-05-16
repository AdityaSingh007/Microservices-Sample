export interface Claim {
  type: string;
  value: string;
}
export type Session = Claim[] | null;
